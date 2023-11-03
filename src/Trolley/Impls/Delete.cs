using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Delete<TEntity> : IDelete<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    protected readonly IDeleteVisitor visitor;
    protected readonly Type entityType;
    private readonly bool isParameterized;
    #endregion

    #region Constructor
    public Delete(TheaConnection connection, IDbTransaction transaction, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.isParameterized = isParameterized;
        this.visitor = ormProvider.NewDeleteVisitor(connection.DbKey, mapProvider, isParameterized);
        this.entityType = typeof(TEntity);
        this.visitor.Initialize(entityType);
    }
    #endregion

    #region Where
    public IDeleted<TEntity> Where(object keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        this.visitor.WhereWith(keys);
        return new Deleted<TEntity>(this.connection, this.transaction, this.visitor);
    }
    public IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IDeleting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return new Deleting<TEntity>(this.connection, this.transaction, this.visitor);
    }
    #endregion
}
class Deleted<TEntity> : IDeleted<TEntity>
{
    #region Fields
    private readonly string dbKey;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDeleteVisitor visitor;
    #endregion

    #region Constructor
    public Deleted(TheaConnection connection, IDbTransaction transaction, IDeleteVisitor visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.ormProvider = visitor.OrmProvider;
        this.mapProvider = visitor.MapProvider;
        this.dbKey = connection.DbKey;
        this.visitor = visitor;
    }
    #endregion

    #region Execute
    public int Execute()
    {
        using var command = this.connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        command.CommandText = this.visitor.BuildCommand(command);
        this.connection.Open();
        var result = command.ExecuteNonQuery();
        command.Dispose();
        return result;
    }
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var cmd = this.connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        cmd.CommandText = this.visitor.BuildCommand(cmd);
        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        await this.connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);
        command.Dispose();
        return result;
    }

    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand() => this.visitor.CreateMultipleCommand();
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        using var command = this.connection.CreateCommand();
        var sql = this.visitor.BuildCommand(command);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion   
}
class Deleting<TEntity> : Deleted<TEntity>, IDeleting<TEntity>
{
    #region Fields
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly IDeleteVisitor visitor;
    #endregion

    #region Constructor
    public Deleting(TheaConnection connection, IDbTransaction transaction, IDeleteVisitor visitor)
        : base(connection, transaction, visitor)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
    }
    #endregion

    #region And
    public IDeleting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition) this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
    #endregion 
}