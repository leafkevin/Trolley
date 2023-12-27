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
    public DbContext DbContext { get; private set; }
    public IUpdateVisitor Visitor { get; private set; }
    #endregion

    #region Constructor
    public Update(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewUpdateVisitor(this.DbContext.DbKey, this.DbContext.MapProvider, this.DbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
    }
    #endregion

    #region Set
    public IContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => this.Set(true, setObj);
    public IContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
    {
        if (setObj == null)
            throw new ArgumentNullException(nameof(setObj));
        if (!typeof(TFields).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数setObj支持实体类对象，不支持基础类型，可以是匿名对、命名对象或是字典");

        if (condition) this.Visitor.SetWith(setObj);
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.Visitor.SetField(fieldSelector, fieldValue);
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.Visitor.Set(fieldsAssignment);
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetFrom    
    public IContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
     => this.SetFrom(true, fieldSelector, valueSelector);
    public IContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.Visitor.SetFrom(fieldSelector, valueSelector);
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    public IContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.Visitor.SetFrom(fieldsAssignment);
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region SetBulk
    public IContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
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
        return new ContinuedUpdate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class Updated<TEntity> : IUpdated<TEntity>, IDisposable
{
    #region Fields
    protected bool hasWhere;
    #endregion

    #region Properties
    public DbContext DbContext { get; private set; }
    public IUpdateVisitor Visitor { get; private set; }
    #endregion

    #region Constructor
    public Updated(DbContext dbContext, IUpdateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        using var command = this.DbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            if (this.Visitor.IsBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                (var updateObjs, var bulkCount, var commandInitializer) = this.Visitor.BuildSetBulk(command);
                foreach (var updateObj in updateObjs)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            this.DbContext.Connection.Open();
                            isFirst = false;
                        }
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
                    if (isFirst) this.DbContext.Connection.Open();
                    result += command.ExecuteNonQuery();
                }
            }
            else
            {
                if (!hasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                this.Visitor.BuildCommand(command);
                this.DbContext.Connection.Open();
                result = command.ExecuteNonQuery();
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
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var command = this.DbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            if (this.Visitor.IsBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                (var updateObjs, var bulkCount, var commandInitializer) = this.Visitor.BuildSetBulk(command);
                foreach (var updateObj in updateObjs)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            await this.DbContext.Connection.OpenAsync(cancellationToken);
                            isFirst = false;
                        }
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
                    if (isFirst) await this.DbContext.Connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
            else
            {
                if (!hasWhere)
                    throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                this.Visitor.BuildCommand(command);
                await this.DbContext.Connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
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
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand() => this.Visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        using var command = this.DbContext.CreateCommand();
        if (this.Visitor.IsBulk)
        {
            int index = 0;
            var sqlBuilder = new StringBuilder();
            (var updateObjs, var bulkCount, var commandInitializer) = this.Visitor.BuildSetBulk(command);
            foreach (var updateObj in updateObjs)
            {
                if (index > 0) sqlBuilder.Append(';');
                commandInitializer.Invoke(sqlBuilder, updateObj, index.ToString());

                if (index >= bulkCount)
                {
                    sql = sqlBuilder.ToString();
                    break;
                }
                index++;
            }
            if (index > 0)
                sql = sqlBuilder.ToString();
        }
        else
        {
            if (!hasWhere)
                throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

            sql = this.Visitor.BuildSql();
        }
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        this.Visitor.Dispose();
        this.DbContext.Dispose();
    }
    #endregion
}
public class ContinuedUpdate<TEntity> : Updated<TEntity>, IContinuedUpdate<TEntity>
{
    #region Constructor
    public ContinuedUpdate(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Set
    public IContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
       => this.Set(true, updateObj);
    public IContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.Visitor.SetWith(updateObj);
        return this;
    }
    public IContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
       => this.Set(true, fieldsAssignment);
    public IContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.Visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    public IContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region IgnoreFields
    public IContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    public IContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess、New或MemberInit类型表达式");

        this.Visitor.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public IContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    public IContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        if (fieldsSelector.Body.NodeType != ExpressionType.New && fieldsSelector.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess、New或MemberInit类型表达式");

        this.Visitor.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.WhereWith(whereObj);
        this.hasWhere = true;
        return this;
    }
    public IContinuedUpdate<TEntity> Where<TWhereObj>(Expression<Func<TEntity, TWhereObj>> whereExpr)
    {
        if (whereExpr == null)
            throw new ArgumentNullException(nameof(whereExpr));

        this.Visitor.Where(whereExpr, true);
        this.hasWhere = true;
        return this;
    }
    public IContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
public class UpdateFrom<TEntity, T1> : Updated<TEntity>, IUpdateFrom<TEntity, T1>
{
    #region Constructor
    public UpdateFrom(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public IUpdateFrom<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateFrom<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateFrom<TEntity, T1, T2> : Updated<TEntity>, IUpdateFrom<TEntity, T1, T2>
{
    #region Constructor
    public UpdateFrom(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateFrom<TEntity, T1, T2, T3> : Updated<TEntity>, IUpdateFrom<TEntity, T1, T2, T3>
{
    #region Constructor
    public UpdateFrom(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateFrom<TEntity, T1, T2, T3, T4> : Updated<TEntity>, IUpdateFrom<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public UpdateFrom(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateFrom<TEntity, T1, T2, T3, T4, T5> : Updated<TEntity>, IUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public UpdateFrom(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region Set
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1> : Updated<TEntity>, IUpdateJoin<TEntity, T1>
{
    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return new UpdateJoin<TEntity, T1, T2>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public IUpdateJoin<TEntity, T1> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2>
{
    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3>
{
    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Set
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
public class UpdateJoin<TEntity, T1, T2, T3, T4, T5> : Updated<TEntity>, IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public UpdateJoin(DbContext dbContext, IUpdateVisitor Visitor)
        : base(dbContext, Visitor) { }
    #endregion

    #region Set
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(TUpdateObj updateObj)
        => this.Set(true, updateObj);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (!typeof(TUpdateObj).IsEntityType(out _))
            throw new NotSupportedException("Set方法参数updateObj支持实体类对象，不支持基础类型，可以是匿名对象或是命名对象或是字典");

        if (condition) this.Visitor.SetWith(updateObj);
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
            this.Visitor.SetField(fieldSelector, fieldValue);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.Visitor.Set(fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.Visitor.SetFrom(fieldSelector, valueSelector);
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
            this.Visitor.SetFrom(fieldsAssignment);
        return this;
    }
    #endregion

    #region Where/And
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> wherePredicate)
        => this.Where(true, wherePredicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> andPredicate)
        => this.And(true, andPredicate);
    public IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion
}
