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
        this.Visitor = this.DbContext.OrmProvider.NewUpdateVisitor(typeof(TEntity), dbContext);
    }
    #endregion

    #region Sharding
    public virtual IUpdate<TEntity> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }  
    public virtual IUpdate<TEntity> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IUpdate<TEntity> UseTable<TUpdateObj>(Func<string, TUpdateObj, string> tableNameGetter)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, tableNameGetter);
        return this;
    }
    public virtual IUpdate<TEntity> UseTableByOthers(params object[] otherFieldValues)
    {
        this.Visitor.UseTableByOthers(TableShardingUsageMode.WriteOnly, otherFieldValues);
        return this;
    }
    public virtual IUpdate<TEntity> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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
    public virtual IBulkContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
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
        return this.OrmProvider.NewBulkContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var shardingType, var shardingTables, var updateObjs, var bulkCount, var fixedSqlSetter, var loopSqlSetter, _) = this.Visitor.BuildWithBulk(command);

                int index = 0;
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
                            loopSqlSetter.Invoke(command.Parameters, builder, tableName, updateObj, index);
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
                                loopSqlSetter.Invoke(command.Parameters, builder, shardingTables as string, updateObj, index);
                                break;
                            case ShardingTableType.MultiTable:
                            case ShardingTableType.ShardingTableMap:
                                var tableNames = shardingTables as List<string>;
                                foreach (var tableName in tableNames)
                                {
                                    loopSqlSetter.Invoke(command.Parameters, builder, tableName, updateObj, index);
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
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();

        switch (this.Visitor.ActionMode)
        {
            case ActionMode.Bulk:
                var builder = new StringBuilder();
                (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, _) = this.Visitor.BuildWithBulk(command);
                Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                Action<object, int> sqlExecuter = null;
                if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                {
                    sqlExecuter = (updateObj, index) =>
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
                    sqlExecuter = (updateObj, index) =>
                    {
                        if (index > 0) builder.Append(';');
                        firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                    };
                }

                int index = 0;
                fixedParameterSetter?.Invoke(command.Parameters);
                await connection.OpenAsync(cancellationToken);
                foreach (var updateObj in updateObjs)
                {
                    sqlExecuter.Invoke(updateObj, index);
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
                break;
            default:
                if (!this.Visitor.HasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");
                command.CommandText = this.Visitor.BuildCommand(this.DbContext, command, out _);
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildCommand(this.DbContext, command, out _);
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        return sql;
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
            if (!this.Visitor.IsMemberVisit(fieldSelector.Body))
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

    #region Where
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
    public virtual IContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
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
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
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
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class BulkContinuedUpdate<TEntity> : Updated<TEntity>, IBulkContinuedUpdate<TEntity>
{
    #region Constructor
    public BulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public virtual IBulkContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
       => this.Set(true, updateObj);
    public virtual IBulkContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (condition)
        {
            if (updateObj == null)
                throw new ArgumentNullException(nameof(updateObj));
            this.Visitor.SetWith(updateObj);
        }
        return this;
    }
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

            this.Visitor.Set(fieldsAssignment);
        }
        return this;
    }
    #endregion

    #region IgnoreFields
    public virtual IBulkContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
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
    public virtual IBulkContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
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
    public virtual IUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.WhereWith(whereObj);
        return this;
    }
    public virtual IBulkContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public virtual IBulkContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IBulkContinuedUpdate<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
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
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
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
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
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
    public virtual IUpdateJoin<TEntity, T1> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Where(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1> WherePredicate(Func<PredicateBuilder<TEntity, T1>, Expression<Func<TEntity, T1, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity, T1>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
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
        var builder = new PredicateBuilder<TEntity, T1>();
        return this.Or(predicateInitializer.Invoke(builder));
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
    public virtual IUpdateJoin<TEntity, T1, T2> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Where(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2>, Expression<Func<TEntity, T1, T2, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity, T1, T2>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
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
        var builder = new PredicateBuilder<TEntity, T1, T2>();
        return this.Or(predicateInitializer.Invoke(builder));
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3>, Expression<Func<TEntity, T1, T2, T3, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity, T1, T2, T3>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
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
        var builder = new PredicateBuilder<TEntity, T1, T2, T3>();
        return this.Or(predicateInitializer.Invoke(builder));
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4>, Expression<Func<TEntity, T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
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
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4>();
        return this.Or(predicateInitializer.Invoke(builder));
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTable(string tableName)
    {
        this.Visitor.UseTable(false, tableName);
        return this;
    }
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
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

    #region Where
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
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
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<TEntity, T1, T2, T3, T4, T5>, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4, T5>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
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
        var builder = new PredicateBuilder<TEntity, T1, T2, T3, T4, T5>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}