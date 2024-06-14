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
        this.Visitor = dbContext.OrmProvider.NewDeleteVisitor(dbContext.DbKey, dbContext.MapProvider, dbContext.ShardingProvider, dbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
    }
    #endregion

    #region Sharding
    public virtual IDelete<TEntity> UseTable(params string[] tableNames)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTable(entityType, tableNames);
        return this;
    }
    public virtual IDelete<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTable(entityType, tableNamePredicate);
        return this;
    }
    public virtual IDelete<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTableBy(entityType, field1Value, field2Value);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTableByRange(entityType, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IDelete<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        var entityType = typeof(TEntity);
        this.Visitor.UseTableByRange(entityType, fieldValue1, fieldValue2, fieldValue3);
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
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.Visitor.Where(ifPredicate);
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
        return this.DbContext.Execute(f =>
        {
            f.CommandText = this.Visitor.BuildCommand(this.DbContext, f);
            return this.Visitor.IsNeedFetchShardingTables;
        });
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);
        return await this.DbContext.ExecuteAsync(f =>
        {
            f.CommandText = this.Visitor.BuildCommand(this.DbContext, f);
            return this.Visitor.IsNeedFetchShardingTables;
        }, cancellationToken);
    }
    #endregion

    #region ToMultipleCommand
    public virtual MultipleCommand ToMultipleCommand() => this.Visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.DbContext.CreateCommand();
        var sql = this.Visitor.BuildCommand(this.DbContext, command);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
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
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion 
}