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
    public DbContext DbContext { get; private set; }
    public ICreateVisitor Visitor { get; private set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public Create(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = this.DbContext.OrmProvider.NewCreateVisitor(this.DbContext.DbKey, this.DbContext.MapProvider, this.DbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
        this.DbContext = dbContext;
    }
    #endregion

    #region WithBy
    public IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulk方法");

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
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T));
        return this.OrmProvider.NewFromCommand<T>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2> From<T1, T2>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2));
        return this.OrmProvider.NewFromCommand<T1, T2>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3> From<T1, T2, T3>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewFromCommand<T1, T2, T3>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.Visitor.CreateQueryVisitor();
        queryVisitor.From('b', null, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, T6>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    #endregion

    #region WithFrom
    public IFromCommand<TTarget> FromWith<TTarget>(IQuery<TTarget> cteSubQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor(true);
        queryVisitor.FromWith(typeof(TTarget), true, cteSubQuery);
        return this.OrmProvider.NewFromCommand<TTarget>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<TTarget> FromWith<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery)
    {
        var queryVisitor = this.Visitor.CreateQueryVisitor(true);
        queryVisitor.FromWith(typeof(TTarget), true, this.DbContext, cteSubQuery);
        return this.OrmProvider.NewFromCommand<TTarget>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    #endregion
}
public class Created<TEntity> : ICreated<TEntity>
{
    #region Properties
    public DbContext DbContext { get; private set; }
    public ICreateVisitor Visitor { get; private set; }
    #endregion

    #region Constructor
    public Created(DbContext dbContext, ICreateVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        int result = 0;
        if (this.Visitor.IsBulk)
        {
            using var command = this.DbContext.CreateCommand();
            bool isNeedClose = this.DbContext.IsNeedClose;
            Exception exception = null;
            try
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                (var insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
                headSqlSetter.Invoke(sqlBuilder);

                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) sqlBuilder.Append(',');
                    commandInitializer.Invoke(sqlBuilder, insertObj, index.ToString());
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
                        headSqlSetter.Invoke(sqlBuilder);
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
                sqlBuilder.Clear();
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
        }
        else
        {
            result = this.DbContext.Execute(f => this.Visitor.BuildCommand(f, true));
            this.Dispose();
        }
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        if (this.Visitor.IsBulk)
        {
            using var command = this.DbContext.CreateDbCommand();
            bool isNeedClose = this.DbContext.IsNeedClose;
            Exception exception = null;
            try
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                (var insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
                headSqlSetter.Invoke(sqlBuilder);
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) sqlBuilder.Append(',');
                    commandInitializer.Invoke(sqlBuilder, insertObj, index.ToString());
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
                        headSqlSetter.Invoke(sqlBuilder);
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
                sqlBuilder.Clear();
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
        }
        else
        {
            result = await this.DbContext.ExecuteAsync(f => this.Visitor.BuildCommand(f, true), cancellationToken);
            this.Dispose();
        }
        return result;
    }
    #endregion

    #region ExecuteIdentity
    public int ExecuteIdentity()
    {
        var result = this.DbContext.CreateIdentity<int>(f => this.Visitor.BuildCommand(f, true));
        this.Dispose();
        return result;
    }
    public async Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.DbContext.CreateIdentityAsync<int>(f => this.Visitor.BuildCommand(f, true), cancellationToken);
        this.Dispose();
        return result;
    }
    public long ExecuteIdentityLong()
    {
        var result = this.DbContext.CreateIdentity<long>(f => this.Visitor.BuildCommand(f, true));
        this.Dispose();
        return result;
    }
    public async Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default)
    {
        var result = await this.DbContext.CreateIdentityAsync<long>(f => this.Visitor.BuildCommand(f, true), cancellationToken);
        this.Dispose();
        return result;
    }
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand()
    {
        var result = this.Visitor.CreateMultipleCommand();
        this.Dispose();
        return result;
    }
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
            (var insertObjs, var bulkCount, var headSqlSetter, var commandInitializer) = this.Visitor.BuildWithBulk(command);
            headSqlSetter.Invoke(sqlBuilder);

            foreach (var insertObj in insertObjs)
            {
                if (index > 0) sqlBuilder.Append(',');
                commandInitializer.Invoke(sqlBuilder, insertObj, index.ToString());
                if (index >= bulkCount)
                {
                    sql = sqlBuilder.ToString();
                    index = 0;
                    break;
                }
                index++;
            }
            if (index > 0) sql = sqlBuilder.ToString();
            sqlBuilder.Clear();
            if (index > 0)
                sql = sqlBuilder.ToString();
        }
        else sql = this.Visitor.BuildCommand(command, false);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion

    public void Dispose()
    {
        this.DbContext.Dispose();
        this.DbContext = null;
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public async ValueTask DisposeAsync()
    {
        await this.DbContext.DisposeAsync();
        this.Visitor.Dispose();
    }
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
