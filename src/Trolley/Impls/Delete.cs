using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Delete : Deleted, IDelete
{
    #region Constructor
    public Delete(Type entityType, DbContext dbContext) : base(dbContext)
    {
        this.Visitor = this.DbContext.OrmProvider.NewDeleteVisitor(entityType, dbContext);
    }
    #endregion

    #region Sharding
    public virtual IDelete UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(TableShardingUsageMode.WriteOnly, false, tableNames);
        return this;
    }
    public virtual IDelete UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    public virtual IDelete UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableShardingUsageMode.WriteOnly, false, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IDelete UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public virtual IDelete WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(false, rawSql);
        return this;
    }
    #endregion

    #region Where
    public virtual IDelete WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public virtual IDelete WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public virtual IDelete WhereById(object whereKey)
        => this.AndById(whereKey);
    public virtual IDelete WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public virtual IDelete WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public virtual IDelete WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    #endregion

    #region And
    public virtual IDelete AndBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.AndBy(whereObj);
        return this;
    }
    public virtual IDelete AndBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.AndBy(whereObj);
    }
    public virtual IDelete AndById(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.AndById(whereKey);
        return this;
    }
    public virtual IDelete AndById(bool condition, object whereKey)
    {
        if (!condition) return this;
        return this.AndById(whereKey);
    }
    public virtual IDelete AndByIds(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.AndByIds(whereKeys);
        return this;
    }
    public virtual IDelete AndByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        return this.AndByIds(whereKeys);
    }
    #endregion

    #region Or
    public virtual IDelete OrBy(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.OrBy(whereObj);
        return this;
    }
    public virtual IDelete OrBy(bool condition, object whereObj)
    {
        if (!condition) return this;
        return this.OrBy(whereObj);
    }
    public virtual IDelete OrById(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.OrById(whereKey);
        return this;
    }
    public virtual IDelete OrById(bool condition, object whereKey)
    {
        if (!condition) return this;
        return this.OrById(whereKey);
    }
    public virtual IDelete OrByIds(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.OrByIds(whereKeys);
        return this;
    }
    public virtual IDelete OrByIds(bool condition, IEnumerable whereKeys)
    {
        if (!condition) return this;
        return this.OrByIds(whereKeys);
    }
    #endregion
}
public class Deleted : DialectProvider, IDeleted
{
    #region Properties
    public IDeleteVisitor Visitor { get; set; }
    #endregion

    #region Constructor
    public Deleted(DbContext dbContext)
    {
        this.DbContext = dbContext;
    }
    #endregion

    #region WithRawSql
    public IDeleted WithLeadingSql(string rawSql)
    {
        this.Visitor.WithLeadingSql(rawSql);
        return this;
    }
    public IDeleted WithTrailingSql(string rawSql)
    {
        this.Visitor.WithTrailingSql(rawSql);
        return this;
    }
    #endregion

    #region Execute
    public virtual int Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        (var isNeedClose, var connection, var command, _) = this.CreateExecuteCommand(this.Visitor);
        var result = this.Execute(isNeedClose, connection, command);
        this.Visitor.Dispose();
        return result;
    }
    public virtual async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        (var isNeedClose, var connection, var command, _) = this.CreateExecuteCommand(this.Visitor);
        var result = await this.ExecuteAsync(isNeedClose, connection, command, cancellationToken);
        this.Visitor.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.UseMasterCommand(this.Visitor.Command);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion
}
public class Delete<TEntity> : Delete, IDelete<TEntity>
{
    #region Constructor
    public Delete(DbContext dbContext) : base(typeof(TEntity), dbContext) { }
    #endregion

    #region Sharding
    public new IDelete<TEntity> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IDelete<TEntity>;
    public new IDelete<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IDelete<TEntity>;
    public new IDelete<TEntity> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IDelete<TEntity>;
    #endregion

    #region UseTableSchema
    public new IDelete<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IDelete<TEntity>;
    #endregion

    #region WithTableAliasTrailing
    public new IDelete<TEntity> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(false, rawSql);
        return this;
    }
    #endregion

    #region Where
    public new IDelete<TEntity> WhereBy(object whereObj)
        => this.AndBy(whereObj);
    public new IDelete<TEntity> WhereBy(bool condition, object whereObj)
        => this.AndBy(condition, whereObj);
    public new IDelete<TEntity> WhereById(object whereKey)
        => this.AndById(whereKey);
    public new IDelete<TEntity> WhereById(bool condition, object whereKey)
        => this.AndById(condition, whereKey);
    public new IDelete<TEntity> WhereByIds(IEnumerable whereKeys)
        => this.AndByIds(whereKeys);
    public new IDelete<TEntity> WhereByIds(bool condition, IEnumerable whereKeys)
        => this.AndByIds(condition, whereKeys);
    public virtual IDelete<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IDelete<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
        => this.And(condition, ifPredicate, elsePredicate);
    public virtual IDelete<TEntity> WherePredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
        => this.AndPredicate(predicateInitializer);
    #endregion

    #region And
    public new IDelete<TEntity> AndBy(object whereObj)
        => base.AndBy(whereObj) as IDelete<TEntity>;
    public new IDelete<TEntity> AndBy(bool condition, object whereObj)
        => base.AndBy(condition, whereObj) as IDelete<TEntity>;
    public new IDelete<TEntity> AndById(object whereKey)
        => base.AndById(whereKey) as IDelete<TEntity>;
    public new IDelete<TEntity> AndById(bool condition, object whereKey)
        => base.AndById(condition, whereKey) as IDelete<TEntity>;
    public new IDelete<TEntity> AndByIds(IEnumerable whereKeys)
        => base.AndByIds(whereKeys) as IDelete<TEntity>;
    public new IDelete<TEntity> AndByIds(bool condition, IEnumerable whereKeys)
        => base.AndByIds(condition, whereKeys) as IDelete<TEntity>;
    public virtual IDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public virtual IDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public new IDelete<TEntity> OrBy(object whereObj)
        => base.OrBy(whereObj) as IDelete<TEntity>;
    public new IDelete<TEntity> OrBy(bool condition, object whereObj)
        => base.OrBy(condition, whereObj) as IDelete<TEntity>;
    public new IDelete<TEntity> OrById(object whereKey)
        => base.OrById(whereKey) as IDelete<TEntity>;
    public new IDelete<TEntity> OrById(bool condition, object whereKey)
        => base.OrById(condition, whereKey) as IDelete<TEntity>;
    public new IDelete<TEntity> OrByIds(IEnumerable whereKeys)
        => base.OrByIds(whereKeys) as IDelete<TEntity>;
    public new IDelete<TEntity> OrByIds(bool condition, IEnumerable whereKeys)
        => base.OrByIds(condition, whereKeys) as IDelete<TEntity>;
    public virtual IDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate)
        => this.Or(true, predicate);
    public virtual IDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
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
    public virtual IDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer)
    {
        if (predicateInitializer == null)
            throw new ArgumentNullException(nameof(predicateInitializer));
        var builder = new PredicateBuilder<TEntity>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion
}
public class ResultDeleted<TResult> : DialectProvider, IBulkResultCommand<TResult>
{
    #region Properties
    public IDeleteVisitor Visitor { get; set; }
    #endregion

    #region Constructor
    public ResultDeleted(DbContext dbContext, IDeleteVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Execute
    public virtual List<TResult> Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command, var readerFields) = this.CreateExecuteCommand(this.Visitor);

        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (reader.Read())
            result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));
        while (reader.NextResult())
        {
            while (reader.Read())
                result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        this.Visitor.Dispose();
        return result;
    }
    public virtual async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command, var readerFields) = this.CreateExecuteCommand(this.Visitor);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (await reader.ReadAsync(cancellationToken))
            result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));
        while (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TResult)readerDeserializer.Invoke(reader, readerFields));
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
    {
        (_, _, var command) = this.UseMasterCommand(this.Visitor.Command);
        var sql = this.Visitor.BuildSql(command, out _);
        dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        this.Visitor.Dispose();
        return sql;
    }
    #endregion
}