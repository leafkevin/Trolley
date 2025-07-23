using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
    public virtual IDelete<TEntity> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
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
    public virtual IContinuedDelete<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        this.Visitor.WhereWith(keys);
        return this.OrmProvider.NewContinuedDelete<TEntity>(this.DbContext, this.Visitor);
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
    public virtual IContinuedDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity>();
        return this.Where(predicateInitializer.Invoke(builder));
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

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out _);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Delete);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out _);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Delete, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region ToMultipleCommand
    public virtual MultipleCommand ToMultipleCommand() => this.Visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var sql = this.Visitor.BuildCommand(command, out _);
        if (this.Visitor.IsNeedFetchShardingTables)
        {
            var builder = new StringBuilder(this.Visitor.BuildTableShardingsSql());
            builder.Append(';');
            builder.Append(sql);
            sql = builder.ToString();
        }
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
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
    public virtual IContinuedDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IContinuedDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IContinuedDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IContinuedDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}