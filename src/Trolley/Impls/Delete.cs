using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Delete<TEntity> : IDelete<TEntity>
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IDeleteVisitor Visitor { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public Delete(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = dbContext.OrmProvider.NewDeleteVisitor(dbContext);
        this.Visitor.Initialize(typeof(TEntity));
    }
    #endregion

    #region Sharding
    public virtual IDelete<TEntity> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IDelete<TEntity> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region Where
    public virtual IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        this.Visitor.WhereWith(keys);
        return this.OrmProvider.NewDeleted<TEntity>(this.DbContext, this.Visitor);
    }
    public virtual IContinuedDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public virtual IContinuedDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this.OrmProvider.NewContinuedDelete<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class Deleted<TEntity> : IDeleted<TEntity>
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IDeleteVisitor Visitor { get; set; }
    #endregion

    #region Constructor
    public Deleted(DbContext dbContext, IDeleteVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public virtual int Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);

        int result = 0;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        try
        {
            command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
            this.DbContext.Open(connection);
            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Delete);
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.DbContext.AddCommandFailedFilter(connection, command, CommandSqlType.Delete, eventArgs, exception);
        }
        finally
        {
            this.DbContext.AddCommandAfterFilter(connection, command, CommandSqlType.Delete, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            command.Dispose();
            if (isNeedClose) this.DbContext.Close(connection);
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

        int result = 0;
        Exception exception = null;
        CommandEventArgs eventArgs = null;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterDbCommand();
        try
        {
            command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
            await this.DbContext.OpenAsync(connection, cancellationToken);
            eventArgs = this.DbContext.AddCommandBeforeFilter(connection, command, CommandSqlType.Delete);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
            this.DbContext.AddCommandFailedFilter(connection, command, CommandSqlType.Delete, eventArgs, exception);
        }
        finally
        {
            this.DbContext.AddCommandAfterFilter(connection, command, CommandSqlType.Delete, eventArgs, exception == null, exception);
            command.Parameters.Clear();
            await command.DisposeAsync();
            if (isNeedClose) await this.DbContext.CloseAsync(connection);
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
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildCommand(this.DbContext, command);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return sql;
    }
    #endregion   
}
public class ContinuedDelete<TEntity> : Deleted<TEntity>, IContinuedDelete<TEntity>
{
    #region Constructor
    public ContinuedDelete(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region And
    public virtual IContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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