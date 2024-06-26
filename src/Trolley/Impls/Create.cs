using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(dbContext.DbKey, dbContext.MapProvider, dbContext.ShardingProvider, dbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
        this.DbContext = dbContext;
    }
    #endregion

    #region Sharding
    public virtual ICreate<TEntity> UseTable(string tableName)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTable(entityType, tableName);
        return this;
    }
    public virtual ICreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTableBy(entityType, field1Value, field2Value);
        return this;
    }
    #endregion

    #region WithBy
    public virtual IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulk方法");

        var insertObjType = typeof(TInsertObject);
        if (!insertObjType.IsEntityType(out _))
            throw new NotSupportedException("单个实体必须是类或结构类型，不能是基础类型");

        this.Visitor.WithBy(insertObj);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public virtual IContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is string || insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");
        bool isEmpty = true;
        foreach (var insertObj in insertObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量插入，insertObjs参数至少要有一条数据");

        this.Visitor.WithBulk(insertObjs, bulkCount);
        return this.OrmProvider.NewContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public virtual IFromCommand<T> From<T>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2> From<T1, T2>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T1), typeof(T2));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T1, T2>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3> From<T1, T2, T3>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T1, T2, T3>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From('a', typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, T6>(typeof(TEntity), this.DbContext, queryVisitor);
    }

    public virtual IFromCommand<TTarget> From<TTarget>(IQuery<TTarget> subQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From(typeof(TTarget), subQuery);
        queryVisitor.IsFromCommand = true;
        return this.OrmProvider.NewFromCommand<TTarget>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public virtual IFromCommand<TTarget> From<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.AddTable(this.Visitor.Tables[0]);
        queryVisitor.From(typeof(TTarget), this.DbContext, cteSubQuery);
        queryVisitor.IsFromCommand = true;
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
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    result = this.DbContext.CreateBulk(this.Visitor);
                    break;
                default:
                    {
                        //默认单条
                        using var command = this.DbContext.CreateCommand();
                        command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                        this.DbContext.Open();
                        result = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                    }
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
        try
        {
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.Bulk:
                    result = await this.DbContext.CreateBulkAsync(this.Visitor, cancellationToken);
                    break;
                default:
                    {
                        //默认单条
                        using var command = this.DbContext.CreateDbCommand();
                        command.CommandText = this.Visitor.BuildCommand(command, false, out _);
                        await this.DbContext.OpenAsync(cancellationToken);
                        result = await command.ExecuteNonQueryAsync(cancellationToken);
                    }
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
        if (condition)
        {
            if (insertObj == null)
                throw new ArgumentNullException(nameof(insertObj));
            if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
                throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");
            if (!typeof(TInsertObject).IsEntityType(out _))
                throw new NotSupportedException("方法WithBy<TInsertObject>(TInsertObject insertObj)只支持类对象参数，不支持基础类型参数");

            this.Visitor.WithBy(insertObj);
        }
        return this;
    }
    public virtual IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.WithBy(true, fieldSelector, fieldValue);
    public virtual IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (condition)
        {
            if (fieldSelector == null)
                throw new ArgumentNullException(nameof(fieldSelector));
            this.Visitor.WithByField(fieldSelector, fieldValue);
        }
        return this;
    }
    #endregion

    #region IgnoreFields
    public virtual IContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.IgnoreFields(fieldNames);
        return this;
    }
    public virtual IContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
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
    public virtual IContinuedCreate<TEntity> OnlyFields(params string[] fieldNames)
    {
        if (fieldNames == null)
            throw new ArgumentNullException(nameof(fieldNames));

        this.Visitor.OnlyFields(fieldNames);
        return this;
    }
    public virtual IContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
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