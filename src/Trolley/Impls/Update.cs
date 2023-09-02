using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Update<TEntity> : IUpdate<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;
    #endregion

    #region Constructor
    public Update(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
    }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Set(fieldsAssignment);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.Set(fieldSelector, fieldValue);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateSetting<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetWith(null, updateObj);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }

    public IUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetFrom(fieldsAssignment);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    public IUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized);
        if (condition) visitor.SetFrom(fieldSelector, valueSelector);
        return new UpdateSetting<TEntity>(this.connection, this.transaction, visitor);
    }
    #endregion

    #region WithBulk
    public IUpdateSet<TEntity> WithBulk<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, IEnumerable updateObjs, int bulkCount = 500)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持New或MemberInit类型表达式");

        Type updateObjType = null;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            if (!updateObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的updateObjs元素类型，updateObjs元素类型可以是字典或是实体类或是多字段元组");
            break;
        }
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .SetBulkFirst(fieldsSelectorOrAssignment, updateObjType);
        return new UpdateSet<TEntity>(this.connection, this.transaction, visitor, updateObjs, bulkCount);
    }
    #endregion

    #region From
    public IUpdateFrom<TEntity, T> From<T>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T));
        return new UpdateFrom<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2> From<T1, T2>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T1), typeof(T2));
        return new UpdateFrom<TEntity, T1, T2>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T1), typeof(T2), typeof(T3));
        return new UpdateFrom<TEntity, T1, T2, T3>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new UpdateFrom<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, visitor);
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new UpdateFrom<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, visitor);
    }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
            .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized)
           .Join("INNER JOIN", typeof(T), joinOn);
        return new UpdateJoin<TEntity, T>(this.connection, this.transaction, visitor);
    }
    #endregion
}
class UpdateSet<TEntity> : IUpdateSet<TEntity>
{
    #region Fields
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly IUpdateVisitor visitor;
    protected readonly IEnumerable updateObjs;
    protected readonly int bulkCount = 500;
    #endregion

    #region Constructor
    public UpdateSet(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor, IEnumerable updateObjs, int bulkCount = 500)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        this.updateObjs = updateObjs;
        this.bulkCount = bulkCount;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        int index = 0, result = 0;
        var sqlBuilder = new StringBuilder();
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        foreach (var updateObj in this.updateObjs)
        {
            if (index > 0) sqlBuilder.Append(';');
            this.visitor.SetBulk(sqlBuilder, command, updateObj, index);

            if (index >= this.bulkCount)
            {
                command.CommandText = sqlBuilder.ToString();
                this.connection.Open();
                result += command.ExecuteNonQuery();
                command.Parameters.Clear();
                sqlBuilder.Clear();
                index = 0;
                continue;
            }
            index++;
        }
        if (index > 0)
        {
            command.CommandText = sqlBuilder.ToString();
            this.connection.Open();
            result += command.ExecuteNonQuery();
        }
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int index = 0, result = 0;
        var sqlBuilder = new StringBuilder();
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        foreach (var updateObj in this.updateObjs)
        {
            if (index > 0) sqlBuilder.Append(';');
            this.visitor.SetBulk(sqlBuilder, command, updateObj, index);

            if (index >= this.bulkCount)
            {
                command.CommandText = sqlBuilder.ToString();
                await this.connection.OpenAsync(cancellationToken);
                result += await command.ExecuteNonQueryAsync(cancellationToken);
                command.Parameters.Clear();
                sqlBuilder.Clear();
                index = 0;
                continue;
            }
            index++;
        }
        if (index > 0)
        {
            command.CommandText = sqlBuilder.ToString();
            await this.connection.OpenAsync(cancellationToken);
            result += await command.ExecuteNonQueryAsync(cancellationToken);
        }
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        int index = 0;
        var sqlBuilder = new StringBuilder();
        using var command = this.connection.CreateCommand();
        foreach (var updateObj in this.updateObjs)
        {
            if (index > 0) sqlBuilder.Append(';');
            this.visitor.SetBulk(sqlBuilder, command, updateObj, index);

            if (index >= this.bulkCount)
            {
                sql = sqlBuilder.ToString();
                index = 0;
                break;
            }
            index++;
        }
        if (index > 0)
            sql = sqlBuilder.ToString();
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion
}
class UpdateBase
{
    #region Fields
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly IUpdateVisitor visitor;
    protected bool hasWhere = false;
    #endregion

    #region Constructor
    public UpdateBase(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        if (!hasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        command.CommandText = this.visitor.BuildSql(out var dbParameters);
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!hasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        cmd.CommandText = this.visitor.BuildSql(out var dbParameters);
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        await command.DisposeAsync();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (!hasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        return this.visitor.BuildSql(out dbParameters);
    }
    #endregion
}
class UpdateSetting<TEntity> : UpdateBase, IUpdateSetting<TEntity>
{
    #region Constructor
    public UpdateSetting(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
       => this.Set(true, fieldsAssignment);
    public IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateSetting<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateSetting<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateSet<TEntity> Where<TFields>(TFields whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.visitor.WhereWith(whereObj);
        this.hasWhere = true;
        return this;
    }
    public IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1> : UpdateBase, IUpdateFrom<TEntity, T1>
{
    #region Constructor
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateFrom<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateFrom<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateFrom<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateFrom<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2> : UpdateBase, IUpdateFrom<TEntity, T1, T2>
{
    #region Constructor
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateFrom<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateFrom<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3>
{
    #region Constructor
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3, T4> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateFrom<TEntity, T1, T2, T3, T4, T5> : UpdateBase, IUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public UpdateFrom(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1> : UpdateBase, IUpdateJoin<TEntity, T1>
{
    #region Constructor
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateJoin<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateJoin<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateJoin<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2> : UpdateBase, IUpdateJoin<TEntity, T1, T2>
{
    #region Constructor
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateJoin<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3>
{
    #region Constructor
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3, T4> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, this.visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : UpdateBase, IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public UpdateJoin(TheaConnection connection, IDbTransaction transaction, IUpdateVisitor visitor)
        : base(connection, transaction, visitor) { }
    #endregion

    #region Set/SetWith/SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}