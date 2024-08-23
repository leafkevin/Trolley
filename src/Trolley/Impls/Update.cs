using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Update<TEntity> : IUpdate<TEntity>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public IUpdateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public Update(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewUpdateVisitor(dbContext);
        this.Visitor.Initialize(typeof(TEntity));
    }
    #endregion

    #region Sharding
    public virtual IUpdate<TEntity> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdate<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdate<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdate<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdate<TEntity> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Set
    public virtual IContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => this.Set(true, setObj);
    public virtual IContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
    {
        if (condition)
        {
            if (setObj == null)
                throw new ArgumentNullException(nameof(setObj));
            if (!typeof(TFields).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数setObj支持实体类对象，不支持基础类型，可以是匿名对、命名对象或是字典");

            this.Visitor.SetWith(setObj);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public virtual IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public virtual IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetFrom    
    public virtual IContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public virtual IContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetBulk
    public virtual IContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        foreach (var updateObj in updateObjs)
        {
            var updateObjType = updateObj.GetType();
            if (!updateObjType.IsEntityType(out _))
                throw new NotSupportedException("批量更新，单个对象类型只支持匿名对象、命名对象或是字典对象");
            break;
        }
        this.Visitor.SetBulk(updateObjs, bulkCount);
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class Updated<TEntity> : IUpdated<TEntity>
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public IUpdateVisitor Visitor { get; protected set; }
    #endregion

    #region Constructor
    public Updated(DbContext dbContext, IUpdateVisitor visitor)
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
        CommandEventArgs eventArgs = null;
        try
        {
            if (this.Visitor.IsNeedFetchShardingTables)
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                        var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            headSqlSetter.Invoke(builder, tableNames[0]);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                headSqlSetter.Invoke(builder, tableNames[i]);
                                sqlSetter.Invoke(builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            headSqlSetter.Invoke(builder, tableName);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    firstParametersSetter?.Invoke(command.Parameters);
                    this.DbContext.Open();
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkUpdate, eventArgs);
                            result += command.ExecuteNonQuery();
                            command.Parameters.Clear();
                            firstParametersSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkUpdate, eventArgs);
                        result += command.ExecuteNonQuery();
                    }
                    builder.Clear();
                    builder = null;
                    break;
                default:
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                    this.DbContext.Open();
                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Update);
                    result = command.ExecuteNonQuery();
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkUpdate : CommandSqlType.Update;
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkUpdate : CommandSqlType.Update;
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
        using var command = this.DbContext.CreateDbCommand();
        CommandEventArgs eventArgs = null;
        try
        {
            bool isOpened = false;
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);
                isOpened = true;
            }
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    var builder = new StringBuilder();
                    (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                        var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            headSqlSetter.Invoke(builder, tableNames[0]);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                headSqlSetter.Invoke(builder, tableNames[i]);
                                sqlSetter.Invoke(builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            headSqlSetter.Invoke(builder, tableName);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.Visitor.OrmProvider, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    firstParametersSetter?.Invoke(command.Parameters);
                    await this.DbContext.OpenAsync(cancellationToken);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkUpdate, eventArgs);
                            result += await command.ExecuteNonQueryAsync(cancellationToken);
                            command.Parameters.Clear();
                            firstParametersSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = builder.ToString();
                        eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.BulkUpdate, eventArgs);
                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    builder.Clear();
                    builder = null;
                    break;
                default:
                    if (!this.Visitor.HasWhere)
                        throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
                    command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                    if (!isOpened) await this.DbContext.OpenAsync(cancellationToken);
                    eventArgs = this.DbContext.AddCommandBeforeFilter(command, CommandSqlType.Update);
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkUpdate : CommandSqlType.Update;
            this.DbContext.AddCommandFailedFilter(command, sqlType, eventArgs, exception);
        }
        finally
        {
            var sqlType = this.Visitor.ActionMode == ActionMode.Bulk ? CommandSqlType.BulkUpdate : CommandSqlType.Update;
            this.DbContext.AddCommandAfterFilter(command, sqlType, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.CloseAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ToMultipleCommand
    public virtual MultipleCommand ToMultipleCommand() => this.Visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.DbContext.CreateCommand();
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        var sql = this.Visitor.BuildCommand(this.DbContext, command);
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
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
public class ContinuedUpdate<TEntity> : Updated<TEntity>, IContinuedUpdate<TEntity>
{
    #region Constructor
    public ContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public virtual IContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
       => this.Set(true, updateObj);
    public virtual IContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
       => this.Set(true, fieldsAssignment);
    public virtual IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region IgnoreFields
    public virtual IContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    public virtual IContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess && fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess、New或MemberInit类型表达式");

        this.Visitor.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public virtual IContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    public virtual IContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess && fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess、New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.WhereWith(whereObj);
        return this;
    }
    public virtual IContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public virtual IContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1> : Updated<TEntity>, IUpdateJoin<TEntity, T1>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdateJoin<TEntity, T1> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Join
    public virtual IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2>(this.DbContext, this.Visitor);
    }
    public virtual IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public virtual IUpdateJoin<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public virtual IUpdateJoin<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            if (!typeof(TUpdateObj).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IUpdateJoin<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public virtual IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> andPredicate)
        => this.And(true, andPredicate);
    public virtual IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Join
    public virtual IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3>(this.DbContext, this.Visitor);
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            if (!typeof(TUpdateObj).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> andPredicate)
        => this.And(true, andPredicate);
    public virtual IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Join
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3, T4>(this.DbContext, this.Visitor);
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3, T4>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            if (!typeof(TUpdateObj).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> andPredicate)
        => this.And(true, andPredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Join
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.DbContext, this.Visitor);
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            if (!typeof(TUpdateObj).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> andPredicate)
        => this.And(true, andPredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Set
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            if (!typeof(TUpdateObj).IsEntityType(out _))
                throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region SetFrom
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetFrom(fieldSelector, valueSelector);
        }
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region Where/And
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> andPredicate)
        => this.And(true, andPredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}