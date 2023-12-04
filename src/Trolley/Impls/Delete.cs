using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Delete<TEntity> : IDelete<TEntity>
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IDeleteVisitor Visitor { get; set; }
    #endregion

    #region Constructor
    public Delete(DbContext dbContext)
    {
        this.DbContext = dbContext;
        this.Visitor = dbContext.OrmProvider.NewDeleteVisitor(dbContext.DbKey, dbContext.MapProvider, dbContext.IsParameterized);
        this.Visitor.Initialize(typeof(TEntity));
    }
    #endregion

    #region Where
    public IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        this.Visitor.WhereWith(keys);
        return new Deleted<TEntity>(this.DbContext, this.Visitor);
    }
    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return new Deleting<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion
}
class Deleted<TEntity> : IDeleted<TEntity>
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
    public int Execute()
        => this.DbContext.Execute(f => this.Visitor.BuildCommand(f));
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.ExecuteAsync(f => this.Visitor.BuildCommand(f), cancellationToken);
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand() => this.Visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.DbContext.CreateCommand();
        var sql = this.Visitor.BuildCommand(command);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion   
}
class Deleting<TEntity> : Deleted<TEntity>, IDeleting<TEntity>
{
    #region Constructor
    public Deleting(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region And
    public IDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion 
}