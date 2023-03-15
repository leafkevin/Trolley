using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class QueryAnonymousObject : IQueryAnonymousObject
{
    private readonly QueryVisitor visitor;
    public QueryAnonymousObject(QueryVisitor visitor)
        => this.visitor = visitor;
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
}
class Query<T> : IQuery<T>
{
    private int unionIndex = 0;
    protected int withIndex = 0;
    protected readonly string dbKey;
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly QueryVisitor visitor;

    public Query(TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor, int withIndex = 0)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        this.dbKey = visitor.dbKey;
        this.withIndex = withIndex;
    }

    #region Union
    public IQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor.Clone('a', $"p{this.unionIndex++}u"));
        var query = subQuery.Invoke(fromQuery);
        var sql = " UNION" + Environment.NewLine + query.ToSql(out var dbParameters);
        this.visitor.Union(typeof(T), sql, dbParameters);
        return this;
    }
    public IQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var fromQuery = new FromQuery(this.visitor.Clone('a', $"p{this.unionIndex++}u"));
        var query = subQuery.Invoke(fromQuery);
        var sql = " UNION ALL" + Environment.NewLine + query.ToSql(out var dbParameters);
        this.visitor.Union(typeof(T), sql, dbParameters);
        return this;
    }
    #endregion

    #region WithCte
    public IQuery<T, TOther> NextWith<TOther>(Func<IFromQuery, IFromQuery<TOther>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        var newVisitor = this.visitor.Clone(tableAsStart, $"p{this.withIndex++}w");
        cteSubQuery.Invoke(new FromQuery(newVisitor));
        var rawSql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        this.visitor.WithCteTable(typeof(TOther), cteTableName, false, rawSql, dbDataParameters, readerFields);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> NextWithRecursive<TOther>(Func<IFromQuery, string, IFromQuery<TOther>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a')
    {
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));

        var newVisitor = this.visitor.Clone(tableAsStart, $"p{this.withIndex++}w");
        cteSubQuery.Invoke(new FromQuery(newVisitor), cteTableName);
        var rawSql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        this.visitor.WithCteTable(typeof(TOther), cteTableName, false, rawSql, dbDataParameters, readerFields);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    #endregion

    #region WithTable
    public IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.withIndex++}w");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters, readerFields);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    #endregion

    #region Join
    public IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.withIndex++}w");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, dbDataParameters, readerFields);
        this.visitor.Join("INNER JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.withIndex++}w");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, dbDataParameters, readerFields);
        this.visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var newVisitor = this.visitor.Clone('a', $"p{this.withIndex++}w");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = newVisitor.BuildSql(out var dbDataParameters, out var readerFields);
        var tableSegment = this.visitor.WithTable(typeof(TOther), sql, dbDataParameters, readerFields);
        this.visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return new Query<T, TOther>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    #endregion

    #region Include
    public IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector);
        return new IncludableQuery<T, TMember>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    public IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.visitor.Include(memberSelector, true, filter);
        return new IncludableQuery<T, TElment>(this.connection, this.transaction, this.visitor, this.withIndex);
    }
    #endregion

    #region Where
    public IQuery<T> Where(Expression<Func<T, bool>> predicate = null)
    {
        if (predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T> Where(bool condition, Expression<Func<T, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T> And(bool condition, Expression<Func<T, bool>> predicate = null)
    {
        if (condition && predicate != null)
            this.visitor.And(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T, TGrouping>(this.connection, this.transaction, this.visitor);
    }
    public IQuery<T> OrderBy(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        this.visitor.OrderBy(rawSql);
        return this;
    }
    public IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Select(null, defaultExpr);
        return this;
    }
    public IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.connection, this.transaction, this.visitor);
    }
    public IQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T> Page(int pageIndex, int pageSize)
    {
        this.visitor.Page(pageIndex, pageSize);
        return this;
    }
    public IQuery<T> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public async Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }
    public long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public async Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    }
    public long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    }

    public TField Sum<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public async Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    }
    public TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public async Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public async Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    public async Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    }

    public T First()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields);
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        T result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            var entityType = typeof(T);
            if (entityType.IsEntityType())
                result = reader.To<T>(this.visitor.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields);
            else result = reader.To<T>();
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields); ;

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        T result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            var entityType = typeof(T);
            if (entityType.IsEntityType())
                result = reader.To<T>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields);
            else result = reader.To<T>();
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public TTarget First<TTarget>(Expression<Func<T, TTarget>> toTargetExpr)
    {
        var entityType = typeof(T);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        var targetType = typeof(TTarget);
        TTarget result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
        {
            if (targetType.IsEntityType())
                result = reader.To<TTarget>(this.visitor.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields);
            else result = reader.To<TTarget>();
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<TTarget> FirstAsync<TTarget>(Expression<Func<T, TTarget>> toTargetExpr, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(T);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var targetType = typeof(TTarget);
        TTarget result = default;
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            if (targetType.IsEntityType())
                result = reader.To<TTarget>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields);
            else result = reader.To<TTarget>();
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }

    public List<T> ToList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields);

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new List<T>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        var entityType = typeof(T);
        if (entityType.IsEntityType())
        {
            while (reader.Read())
            {
                result.Add(reader.To<T>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<T>());
            }
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<T>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        var entityType = typeof(T);
        if (entityType.IsEntityType())
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<T>(this.visitor.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<T>());
            }
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public List<TTarget> ToList<TTarget>(Expression<Func<T, TTarget>> toTargetExpr)
    {
        var entityType = typeof(TTarget);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new List<TTarget>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (entityType.IsEntityType())
        {
            while (reader.Read())
            {
                result.Add(reader.To<TTarget>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TTarget>());
            }
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<List<TTarget>> ToListAsync<TTarget>(Expression<Func<T, TTarget>> toTargetExpr, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TTarget);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new List<TTarget>();
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        if (entityType.IsEntityType())
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TTarget>(this.visitor.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<TTarget>());
            }
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }

    public IPagedList<T> ToPageList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields);

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        int recordsTotal = 0;
        var result = new PagedList<T>();
        result.Items = new List<T>();

        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
            recordsTotal = reader.To<int>();

        reader.NextResult();
        while (reader.Read())
        {
            result.Items.Add(reader.To<T>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
        }
        reader.Close();
        reader.Dispose();
        result.RecordsTotal = recordsTotal;

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, null, null, out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        int recordsTotal = 0;
        var result = new PagedList<T>();
        result.Items = new List<T>();

        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync())
            recordsTotal = reader.To<int>();

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            result.Items.Add(reader.To<T>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        result.RecordsTotal = recordsTotal;

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public IPagedList<TTarget> ToPageList<TTarget>(Expression<Func<T, TTarget>> toTargetExpr)
    {
        var entityType = typeof(TTarget);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        int recordsTotal = 0;
        var result = new PagedList<TTarget>();
        result.Items = new List<TTarget>();

        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
            recordsTotal = reader.To<int>();

        reader.NextResult();
        while (reader.Read())
        {
            result.Items.Add(reader.To<TTarget>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
        }
        reader.Close();
        reader.Dispose();
        result.RecordsTotal = recordsTotal;

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<IPagedList<TTarget>> ToPageListAsync<TTarget>(Expression<Func<T, TTarget>> toTargetExpr, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TTarget);
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, entityType, toTargetExpr, out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        int recordsTotal = 0;
        var result = new PagedList<TTarget>();
        result.Items = new List<TTarget>();

        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync())
            recordsTotal = reader.To<int>();

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            result.Items.Add(reader.To<TTarget>(this.dbKey, this.visitor.ormProvider, this.visitor.isTarget, readerFields));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        result.RecordsTotal = recordsTotal;

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }

    public Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.ToList().ToDictionary(keySelector, valueSelector);
    }
    public async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        var list = await this.ToListAsync(cancellationToken);
        return list.ToDictionary(keySelector, valueSelector);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, null, null, out dbParameters, out _);
    }

    private TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out _);
        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        TTarget result = default;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<TTarget>();
        reader.Close();
        reader.Dispose();
        command.Dispose();
        return result;
    }
    private async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out _);
        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        TTarget result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await this.connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.To<TTarget>();
        await reader.CloseAsync();
        await reader.DisposeAsync();
        await command.DisposeAsync();
        return result;
    }
}