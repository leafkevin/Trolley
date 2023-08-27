using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

class MultiCreate<TEntity> : IMultiCreate<TEntity>
{
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;

    public MultiCreate(MultipleQuery multiQuery)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.isParameterized = multiQuery.IsParameterized;
    }
    public IMultiCreated<TEntity> RawSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return new MultiCreated<TEntity>(this.multiQuery).RawSql(rawSql);
    }
    public IMultiCreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return new MultiCreated<TEntity>(this.multiQuery).RawSql(rawSql, parameters);
    }
    public IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not Dictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");

        return new MultiContinuedCreate<TEntity>(this.multiQuery).WithBy(insertObj);
    }
    public IMultiCreated<TEntity> WithBulk<TInsertObject>(IEnumerable<TInsertObject> insertObjs)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        return new MultiCreated<TEntity>(this.multiQuery).WithBulk(insertObjs);
    }
    public IMultiContinuedCreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new MultiContinuedCreate<TEntity, TSource>(this.multiQuery, visitor);
    }
    public IMultiContinuedCreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new MultiContinuedCreate<TEntity, T1, T2>(this.multiQuery, visitor);
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new MultiContinuedCreate<TEntity, T1, T2, T3>(this.multiQuery, visitor);
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new MultiContinuedCreate<TEntity, T1, T2, T3, T4>(this.multiQuery, visitor);
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));

        var entityType = typeof(TEntity);
        var visitor = this.ormProvider.NewCreateVisitor(this.connection.DbKey, this.mapProvider, entityType, this.isParameterized).From(fieldSelector);
        return new MultiContinuedCreate<TEntity, T1, T2, T3, T4, T5>(this.multiQuery, visitor);
    }
}
class MultiContinuedCreate<TEntity> : IMultiContinuedCreate<TEntity>
{
    private readonly List<WithByBuilderCache> builders = new();
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbCommand command;

    public MultiContinuedCreate(MultipleQuery multiQuery)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.command = multiQuery.Command;
    }
    public IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        if (insertObj is IEnumerable && insertObj is not string && insertObj is not Dictionary<string, object>)
            throw new NotSupportedException("只能插入单个实体，批量插入请使用WithBulkBy方法");

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildCreateWithBiesCommandInitializer(
              this.connection, this.ormProvider, this.mapProvider, entityType, insertObj);

        this.builders.Add(new WithByBuilderCache
        {
            CommandInitializer = commandInitializer,
            Parameters = insertObj
        });
        return this;
    }
    public IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj)
    {
        if (condition) this.WithBy(insertObj);
        return this;
    }
    //public IMultiContinuedCreate<TEntity> IgnoreInto ( )
    //{

    //    return this;
    //}
    public IMultipleQuery Execute()
    {
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        var sql = this.BuildSql(entityMapper, this.command);
        this.builders.Clear();
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        var entityType = typeof(TEntity);
        var entityMapper = this.mapProvider.GetEntityMap(entityType);
        using var command = this.connection.CreateCommand();
        var sql = this.BuildSql(entityMapper, command);

        dbParameters = null;
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Cancel();
        command.Dispose();
        return sql;
    }
    private string BuildSql(EntityMap entityMapper, IDbCommand command)
    {
        var insertBuilder = new StringBuilder($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
        int index = 0;
        foreach (var builder in this.builders)
        {
            if (index > 0)
            {
                insertBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            builder.CommandInitializer.Invoke(command, this.ormProvider, this.mapProvider, builder.Parameters, insertBuilder, valuesBuilder);
            index++;
        }
        insertBuilder.Append(')');
        valuesBuilder.Append(')');

        if (entityMapper.IsAutoIncrement)
            valuesBuilder.AppendFormat(this.ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
        return insertBuilder.ToString() + valuesBuilder.ToString();
    }
    struct WithByBuilderCache
    {
        public object Parameters { get; set; }
        public Action<IDbCommand, IOrmProvider, IEntityMapProvider, object, StringBuilder, StringBuilder> CommandInitializer { get; set; }
    }
}
class MultiCreated<TEntity> : IMultiCreated<TEntity>
{
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly IDbCommand command;
    private string rawSql = null;
    private object parameters = null;

    public MultiCreated(MultipleQuery multiQuery)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.command = multiQuery.Command;
    }
    public IMultiCreated<TEntity> RawSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.rawSql = rawSql;
        return this;
    }
    public IMultiCreated<TEntity> RawSql(string rawSql, object parameters)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        this.rawSql = rawSql;
        this.parameters = parameters;
        return this;
    }
    public IMultiCreated<TEntity> WithBulk(IEnumerable insertObjs)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        this.parameters = insertObjs;
        return this;
    }
    public IMultipleQuery Execute()
    {
        var entityType = typeof(TEntity);
        var sql = this.BuildSql(entityType, this.command);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        var entityType = typeof(TEntity);
        using var command = this.connection.CreateCommand();
        string sql = this.BuildSql(entityType, command);
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    private string BuildSql(Type entityType, IDbCommand command)
    {
        string sql = null;
        if (!string.IsNullOrEmpty(this.rawSql))
        {
            if (this.parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildCreateRawSqlParameters(
                    this.connection, this.ormProvider, this.mapProvider, entityType, this.rawSql, this.parameters);
                commandInitializer.Invoke(command, this.ormProvider, this.parameters);
            }
            sql = this.rawSql;
        }
        else
        {
            int index = 0;
            var entities = this.parameters as IEnumerable;
            var commandInitializer = RepositoryHelper.BuildCreateBatchCommandInitializer(this.connection, this.ormProvider, this.mapProvider, entityType, this.parameters);
            var sqlBuilder = new StringBuilder();
            foreach (var entity in entities)
            {
                commandInitializer.Invoke(command, this.ormProvider, this.mapProvider, sqlBuilder, index, entity);
                index++;
            }
            sql = sqlBuilder.ToString();
        }
        return sql;
    }
}
class MultiContinuedCreateBase
{
    protected MultipleQuery multiQuery;
    protected ICreateVisitor visitor;

    public MultiContinuedCreateBase(MultipleQuery multiQuery, ICreateVisitor visitor)
    {
        this.multiQuery = multiQuery;
        this.visitor = visitor;
    }
    public IMultipleQuery Execute()
    {
        var sql = this.visitor.BuildSql(out var dbParameters);
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter, dbParameters);
        return this.multiQuery;
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters);
}
class MultiContinuedCreate<TEntity, TSource> : MultiContinuedCreateBase, IMultiContinuedCreate<TEntity, TSource>
{
    public MultiContinuedCreate(MultipleQuery multiQuery, ICreateVisitor visitor)
        : base(multiQuery, visitor) { }
    public IMultiContinuedCreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiContinuedCreate<TEntity, TSource> Where(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiContinuedCreate<TEntity, TSource> And(Expression<Func<TSource, bool>> predicate)
        => this.And(true, predicate);
    public IMultiContinuedCreate<TEntity, TSource> And(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class MultiContinuedCreate<TEntity, T1, T2> : MultiContinuedCreateBase, IMultiContinuedCreate<TEntity, T1, T2>
{
    public MultiContinuedCreate(MultipleQuery multiQuery, ICreateVisitor visitor)
        : base(multiQuery, visitor) { }
    public IMultiContinuedCreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiContinuedCreate<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class MultiContinuedCreate<TEntity, T1, T2, T3> : MultiContinuedCreateBase, IMultiContinuedCreate<TEntity, T1, T2, T3>
{
    public MultiContinuedCreate(MultipleQuery multiQuery, ICreateVisitor visitor)
        : base(multiQuery, visitor) { }
    public IMultiContinuedCreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class MultiContinuedCreate<TEntity, T1, T2, T3, T4> : MultiContinuedCreateBase, IMultiContinuedCreate<TEntity, T1, T2, T3, T4>
{
    public MultiContinuedCreate(MultipleQuery multiQuery, ICreateVisitor visitor)
        : base(multiQuery, visitor) { }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}
class MultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> : MultiContinuedCreateBase, IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5>
{
    public MultiContinuedCreate(MultipleQuery multiQuery, ICreateVisitor visitor)
        : base(multiQuery, visitor) { }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        return this;
    }
}