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

public class Update : IUpdate
{
    #region Properties
    public DbContext DbContext { get; protected set; }
    public IUpdateVisitor Visitor { get; protected set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public Update(Type entityType, DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewUpdateVisitor(entityType, dbContext);
    }
    #endregion

    #region Sharding
    public virtual IUpdate UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IUpdate UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdate UseTable(Func<object, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
        return this;
    }
    public virtual IUpdate UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IUpdate UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IUpdate WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(false, rawSql);
        return this;
    }
    #endregion

    #region Set
    public virtual IContinuedUpdate Set(object updateObj)
        => this.Set(true, updateObj);
    public virtual IContinuedUpdate Set(bool condition, object updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            var type = updateObj.GetType();
            if (!type.IsEntityType(out _))
                throw new NotSupportedException("Set方法参数setObj支持实体类对象，不支持基础类型，可以是匿名对、命名对象或是字典");
            this.Visitor.SetObject(updateObj);
        }
        return this.OrmProvider.NewContinuedUpdate(this.DbContext, this.Visitor);
    }
    public virtual IContinuedUpdate Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public virtual IContinuedUpdate Set(bool condition, string fieldName, object fieldValue)
    {
        if (condition)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            this.Visitor.SetField(fieldName, fieldValue);
        }
        return this.OrmProvider.NewContinuedUpdate(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetBulk
    public virtual IBulkContinuedUpdate SetBulk(IEnumerable updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        bool isEmpty = true;
        foreach (var updateObj in updateObjs)
        {
            var updateObjType = updateObj.GetType();
            if (!updateObjType.IsEntityType(out _))
                throw new NotSupportedException("批量更新，单个对象类型只支持匿名对象、命名对象或是字典对象");
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");

        this.Visitor.SetBulk(updateObjs, bulkCount);
        return this.OrmProvider.NewBulkContinuedUpdate(this.DbContext, this.Visitor);
    }
    #endregion
}
public class Updated : IUpdated
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

    #region WithRawSql
    public IUpdated WithLeadingSql(string rawSql)
    {
        this.Visitor.WithLeadingSql(rawSql);
        return this;
    }
    public IUpdated WithTrailingSql(string rawSql)
    {
        this.Visitor.WithTrailingSql(rawSql);
        return this;
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
                (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                    var fixedSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildSetBulk(command);

                int index = 0;
                var builder = new StringBuilder();
                fixedSqlSetter?.Invoke(command.Parameters);

                connection.Open();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledUpdateObjs.Keys)
                    {
                        var tableParameters = tabledUpdateObjs[tableName];
                        foreach (var updateObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                        }
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                            command.Parameters.Clear();
                            fixedSqlSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                }
                else
                {
                    foreach (var updateObj in updateObjs)
                    {
                        switch (shardingType)
                        {
                            case ShardingTableType.None:
                            case ShardingTableType.SingleTable:
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                break;
                            case ShardingTableType.MultiTable:
                            case ShardingTableType.ShardingTableMap:
                                var tableNames = shardingTables as List<string>;
                                foreach (var tableName in tableNames)
                                {
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                }
                                break;
                        }
                        index++;
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                            command.Parameters.Clear();
                            fixedSqlSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
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
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");
                command.CommandText = this.Visitor.BuildSql(command, out _);
                connection.Open();
                result = command.ExecuteNonQuery(CommandSqlType.Update);
                break;
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);

        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var updateObjs, var bulkCount,
                    var fixedSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildSetBulk(command);

                int index = 0;
                var builder = new StringBuilder();
                fixedSqlSetter?.Invoke(command.Parameters);

                connection.Open();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledUpdateObjs.Keys)
                    {
                        var tableParameters = tabledUpdateObjs[tableName];
                        foreach (var updateObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                        }
                        index++;

                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                            command.Parameters.Clear();
                            fixedSqlSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
                }
                else
                {
                    foreach (var updateObj in updateObjs)
                    {
                        switch (shardingType)
                        {
                            case ShardingTableType.None:
                            case ShardingTableType.SingleTable:
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                break;
                            case ShardingTableType.MultiTable:
                            case ShardingTableType.ShardingTableMap:
                                var tableNames = shardingTables as List<string>;
                                foreach (var tableName in tableNames)
                                {
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                }
                                break;
                        }
                        index++;
                        if (index >= bulkCount)
                        {
                            command.CommandText = builder.ToString();
                            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                            command.Parameters.Clear();
                            fixedSqlSetter?.Invoke(command.Parameters);
                            builder.Clear();
                            index = 0;
                        }
                    }
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
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");
                command.CommandText = this.Visitor.BuildSql(command, out _);
                await connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
                break;
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion
}
public class ContinuedUpdate : Updated, IContinuedUpdate
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public ContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public virtual IContinuedUpdate Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public virtual IContinuedUpdate Set(bool condition, string fieldName, object fieldValue)
    {
        if (condition)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            this.Visitor.SetField(fieldName, fieldValue);
        }
        return this.OrmProvider.NewContinuedUpdate(this.DbContext, this.Visitor);
    }
    #endregion

    #region IgnoreFields
    public virtual IContinuedUpdate IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    #endregion

    #region OnlyFields
    public virtual IContinuedUpdate OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    #endregion

    #region Where
    public virtual IContinuedUpdate WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public virtual IContinuedUpdate WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public virtual IContinuedUpdate WhereById(object whereKey)
        => this.AndById(whereKey);
    public virtual IContinuedUpdate WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public virtual IContinuedUpdate WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public virtual IContinuedUpdate WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    #endregion

    #region And
    public virtual IContinuedUpdate AndBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.AndBy(whereObj);
        return this;
    }
    public virtual IContinuedUpdate AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.AndBy(whereObj);
    }
    public virtual IContinuedUpdate AndById(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.AndById(whereKey);
        return this;
    }
    public virtual IContinuedUpdate AndById(bool condition, object whereKey)
    {
        if (!condition) return this;
        return this.AndById(whereKey);
    }
    public virtual IContinuedUpdate AndByIds(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.AndByIds(whereKeys);
        return this;
    }
    public virtual IContinuedUpdate AndByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        return this.AndByIds(whereKeys);
    }
    #endregion

    #region Or
    public virtual IContinuedUpdate OrBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.OrBy(whereObj);
        return this;
    }
    public virtual IContinuedUpdate OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.OrBy(whereObj);
    }
    public virtual IContinuedUpdate OrById(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.OrById(whereKey);
        return this;
    }
    public virtual IContinuedUpdate OrById(bool condition, object whereKey)
    {
        if (!condition) return this;
        return this.OrById(whereKey);
    }
    public virtual IContinuedUpdate OrByIds(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.OrByIds(whereKeys);
        return this;
    }
    public virtual IContinuedUpdate OrByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        return this.OrByIds(whereKeys);
    }
    #endregion
}
public class BulkContinuedUpdate : Updated, IBulkContinuedUpdate
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public BulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public virtual IBulkContinuedUpdate Set(object updateObj)
         => this.Set(true, updateObj);
    public virtual IBulkContinuedUpdate Set(bool condition, object updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            var type = updateObj.GetType();
            if (!type.IsEntityType(out _))
                throw new NotSupportedException("Set方法参数setObj支持实体类对象，不支持基础类型，可以是匿名对、命名对象或是字典");
            this.Visitor.SetObject(updateObj);
        }
        return this.OrmProvider.NewBulkContinuedUpdate(this.DbContext, this.Visitor);
    }
    public virtual IBulkContinuedUpdate Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public virtual IBulkContinuedUpdate Set(bool condition, string fieldName, object fieldValue)
    {
        if (condition)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            this.Visitor.SetField(fieldName, fieldValue);
        }
        return this.OrmProvider.NewBulkContinuedUpdate(this.DbContext, this.Visitor);
    }
    #endregion

    #region IgnoreFields
    public virtual IBulkContinuedUpdate IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    #endregion

    #region OnlyFields
    public virtual IBulkContinuedUpdate OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    #endregion

    #region Where
    public virtual IBulkContinuedUpdate WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public virtual IBulkContinuedUpdate WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    #endregion

    #region And
    public virtual IBulkContinuedUpdate AndBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.AndBy(whereObj);
        return this;
    }
    public virtual IBulkContinuedUpdate AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.AndBy(whereObj);
    }
    #endregion

    #region Or
    public virtual IBulkContinuedUpdate OrBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.OrBy(whereObj);
        return this;
    }
    public virtual IBulkContinuedUpdate OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.OrBy(whereObj);
    }
    #endregion
}
public class BulkCopyContinuedUpdate : Updated, IBulkCopyContinuedUpdate
{
    #region Constructor
    public BulkCopyContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Where
    public virtual IBulkCopyContinuedUpdate WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public virtual IBulkCopyContinuedUpdate WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    #endregion

    #region And
    public virtual IBulkCopyContinuedUpdate AndBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.AndBy(whereObj);
        return this;
    }
    public virtual IBulkCopyContinuedUpdate AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.AndBy(whereObj);
    }
    #endregion

    #region Or
    public virtual IBulkCopyContinuedUpdate OrBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.OrBy(whereObj);
        return this;
    }
    public virtual IBulkCopyContinuedUpdate OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.OrBy(whereObj);
    }
    #endregion
}
public class Update<TEntity> : Update, IUpdate<TEntity>
{
    #region Constructor
    public Update(DbContext dbContext) : base(typeof(TEntity), dbContext) { }
    #endregion

    #region Sharding
    public new IUpdate<TEntity> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IUpdate<TEntity>;
    public new IUpdate<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IUpdate<TEntity>;
    public new IUpdate<TEntity> UseTable(Func<object, string> tableNameGetter)
        => base.UseTable(tableNameGetter) as IUpdate<TEntity>;
    public new IUpdate<TEntity> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IUpdate<TEntity>;
    #endregion

    #region UseTableSchema
    public new IUpdate<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IUpdate<TEntity>;
    #endregion

    #region Set
    public new IContinuedUpdate<TEntity> Set(object updateObj)
        => this.Set(true, updateObj);
    public new IContinuedUpdate<TEntity> Set(bool condition, object updateObj)
        => base.Set(condition, updateObj) as IContinuedUpdate<TEntity>;
    public new IContinuedUpdate<TEntity> Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public new IContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue)
        => base.Set(condition, fieldName, fieldValue) as IContinuedUpdate<TEntity>;

    public IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetExpr(fieldsAssignment);
        }
        return this.OrmProvider.NewContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetFrom
    public virtual IBulkContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public virtual IBulkContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
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
        return this.OrmProvider.NewBulkContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public virtual IBulkContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public virtual IBulkContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetFrom(fieldsAssignment);
        }
        return this.OrmProvider.NewBulkContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetBulk
    public new IBulkContinuedUpdate<TEntity> SetBulk(IEnumerable updateObjs, int bulkCount = 500)
        => base.SetBulk(updateObjs, bulkCount) as IBulkContinuedUpdate<TEntity>;
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        this.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        this.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class ResultUpdated<TResult> : IBulkResultCommand<TResult>
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IUpdateVisitor Visitor { get; set; }
    #endregion

    #region Constructor
    public ResultUpdated(DbContext dbContext, IUpdateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public virtual List<TResult> Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        command.CommandText = this.Visitor.BuildSql(command, out var readerFields);
        connection.Open();

        using var reader = command.ExecuteReader(CommandSqlType.Update, CommandBehavior.SequentialAccess);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (reader.Read())
            result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public virtual async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand(this.Visitor);
        command.CommandText = this.Visitor.BuildSql(command, out var readerFields);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Update, CommandBehavior.SequentialAccess, cancellationToken);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (await reader.ReadAsync(cancellationToken))
            result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
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
        return sql;
    }
    #endregion
}


public class ContinuedUpdate<TEntity> : ContinuedUpdate, IContinuedUpdate<TEntity>
{
    #region Constructor
    public ContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public new IContinuedUpdate<TEntity> Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public new IContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue)
        => base.Set(condition, fieldName, fieldValue) as IContinuedUpdate<TEntity>;

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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
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
    public new IContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IContinuedUpdate<TEntity>;
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
    public new IContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.OnlyFields(fieldNames) as IContinuedUpdate<TEntity>;
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

    #region Where
    public new IContinuedUpdate<TEntity> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public new IContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public new IContinuedUpdate<TEntity> WhereById(object whereKey)
        => this.AndById(whereKey);
    public new IContinuedUpdate<TEntity> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public new IContinuedUpdate<TEntity> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public new IContinuedUpdate<TEntity> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public virtual IContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public new IContinuedUpdate<TEntity> AndBy(object whereObj)
        => this.AndBy(true, whereObj);
    public new IContinuedUpdate<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IContinuedUpdate<TEntity>;
    public new IContinuedUpdate<TEntity> AndById(object whereKey)
        => this.AndById(true, whereKey);
    public new IContinuedUpdate<TEntity> AndById(bool condition, object whereKey)
        => base.AndById(condition, whereKey) as IContinuedUpdate<TEntity>;
    public new IContinuedUpdate<TEntity> AndByIds(IEnumerable whereKeys)
        => this.AndByIds(true, whereKeys);
    public new IContinuedUpdate<TEntity> AndByIds(bool condition, IEnumerable whereKeys)
        => base.AndByIds(condition, whereKeys) as IContinuedUpdate<TEntity>;
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
    public virtual IContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public new IContinuedUpdate<TEntity> OrBy(object whereObj)
        => this.OrBy(true, whereObj);
    public new IContinuedUpdate<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IContinuedUpdate<TEntity>;
    public new IContinuedUpdate<TEntity> OrById(object whereKey)
        => this.OrById(true, whereKey);
    public new IContinuedUpdate<TEntity> OrById(bool condition, object whereKey)
        => base.OrById(condition, whereKey) as IContinuedUpdate<TEntity>;
    public new IContinuedUpdate<TEntity> OrByIds(IEnumerable whereKeys)
        => this.OrByIds(true, whereKeys);
    public new IContinuedUpdate<TEntity> OrByIds(bool condition, IEnumerable whereKeys)
        => base.OrByIds(condition, whereKeys) as IContinuedUpdate<TEntity>;
    public virtual IContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class BulkContinuedUpdate<TEntity> : BulkContinuedUpdate, IBulkContinuedUpdate<TEntity>
{
    #region Constructor
    public BulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public new IBulkContinuedUpdate<TEntity> Set(object updateObj)
        => this.Set(true, updateObj);
    public new IBulkContinuedUpdate<TEntity> Set(bool condition, object updateObj)
        => base.Set(condition, updateObj) as IBulkContinuedUpdate<TEntity>;
    public new IBulkContinuedUpdate<TEntity> Set(string fieldName, object fieldValue)
        => this.Set(true, fieldName, fieldValue);
    public new IBulkContinuedUpdate<TEntity> Set(bool condition, string fieldName, object fieldValue)
        => base.Set(condition, fieldName, fieldValue) as IBulkContinuedUpdate<TEntity>;
    public virtual IBulkContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public virtual IBulkContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            if (fieldValue == null)
                throw new ArgumentNullException(nameof(fieldValue));
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
                throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

            this.Visitor.SetField(fieldSelector, fieldValue);
        }
        return this;
    }
    public virtual IBulkContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
       => this.Set(true, fieldsAssignment);
    public virtual IBulkContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (condition)
        {
            if (fieldsAssignment == null)
                throw new ArgumentNullException(nameof(fieldsAssignment));
            if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
                throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

            this.Visitor.SetExpr(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region IgnoreFields
    public new IBulkContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IBulkContinuedUpdate<TEntity>;
    public virtual IBulkContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
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
    public new IBulkContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
        => base.IgnoreFields(fieldNames) as IBulkContinuedUpdate<TEntity>;
    public virtual IBulkContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess && fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess、New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Where
    public new IBulkContinuedUpdate<TEntity> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public new IBulkContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public virtual IBulkContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IBulkContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IBulkContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public new IBulkContinuedUpdate<TEntity> AndBy(object whereObj)
        => this.AndBy(true, whereObj);
    public new IBulkContinuedUpdate<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IBulkContinuedUpdate<TEntity>;
    public virtual IBulkContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IBulkContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IBulkContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public new IBulkContinuedUpdate<TEntity> OrBy(object whereObj)
        => this.OrBy(true, whereObj);
    public new IBulkContinuedUpdate<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IBulkContinuedUpdate<TEntity>;
    public virtual IBulkContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IBulkContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IBulkContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class BulkCopyContinuedUpdate<TEntity> : BulkCopyContinuedUpdate, IBulkCopyContinuedUpdate<TEntity>
{
    #region Constructor
    public BulkCopyContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Where
    public new IBulkCopyContinuedUpdate<TEntity> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public new IBulkCopyContinuedUpdate<TEntity> WhereBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IBulkCopyContinuedUpdate<TEntity>;
    public virtual IBulkCopyContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IBulkCopyContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IBulkCopyContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public new IBulkCopyContinuedUpdate<TEntity> AndBy(object whereObj)
        => this.AndBy(true, whereObj);
    public new IBulkCopyContinuedUpdate<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IBulkCopyContinuedUpdate<TEntity>;
    public virtual IBulkCopyContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IBulkCopyContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IBulkCopyContinuedUpdate<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public new IBulkCopyContinuedUpdate<TEntity> OrBy(object whereObj)
        => this.OrBy(true, whereObj);
    public new IBulkCopyContinuedUpdate<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IBulkCopyContinuedUpdate<TEntity>;
    public virtual IBulkCopyContinuedUpdate<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IBulkCopyContinuedUpdate<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IBulkCopyContinuedUpdate<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class UpdateJoin<TEntity, T1> : Updated, IUpdateJoin<TEntity, T1>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
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

            this.Visitor.SetObject(updateObj);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

            this.Visitor.SetExpr(fieldsAssignment);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
    public virtual IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IUpdateJoin<TEntity, T1> WherePredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1> AndPredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IUpdateJoin<TEntity, T1> Or(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IUpdateJoin<TEntity, T1> Or(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> OrPredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2> : Updated, IUpdateJoin<TEntity, T1, T2>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
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

            this.Visitor.SetObject(updateObj);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

            this.Visitor.SetExpr(fieldsAssignment);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IUpdateJoin<TEntity, T1, T2> Or(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2> Or(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3> : Updated, IUpdateJoin<TEntity, T1, T2, T3>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
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

            this.Visitor.SetObject(updateObj);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

            this.Visitor.SetExpr(fieldsAssignment);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Or(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4> : Updated, IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
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

            this.Visitor.SetObject(updateObj);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

            this.Visitor.SetExpr(fieldsAssignment);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Or(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : Updated, IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    #region Properties
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Sharding
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(string tableName)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(TableShardingUsageMode.WriteOnly, false, tableNameGetter);
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

            this.Visitor.SetObject(updateObj);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

            this.Visitor.SetExpr(fieldsAssignment);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4, T5>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Or(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4, T5>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}