using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Query<T> : IQuery<T>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    protected readonly IOrmDbFactory dbFactory;
    protected readonly TheaConnection connection;
    protected readonly IDbTransaction transaction;
    protected readonly QueryVisitor visitor;

    public Query(QueryVisitor visitor)
    {
        this.visitor = visitor;
        this.dbFactory = visitor.dbFactory;
        this.connection = visitor.connection;
        this.transaction = visitor.transaction;
    }

    #region Include
    public IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T, TMember>(this.visitor);
    }
    public IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, true, filter);
        return new IncludableQuery<T, TElment>(this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery(this.dbFactory, this.connection, this.transaction, $"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T, TOther>(this.visitor);
    }
    public IQuery<T> InnerJoin(Expression<Func<T, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", null, joinOn);
        return this;
    }
    public IQuery<T> LeftJoin(Expression<Func<T, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", null, joinOn);
        return this;
    }
    public IQuery<T> RightJoin(Expression<Func<T, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", null, joinOn);
        return this;
    }
    public IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.visitor);
    }
    public IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.visitor);
    }
    public IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new Query<T, TOther>(this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.dbFactory, this.connection, this.transaction, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T), sql, dbParameters);
        return this;
    }
    public IQuery<T> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery(this.dbFactory, this.connection, this.transaction, $"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T> And(bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition)
            this.visitor.And(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T, TGrouping>(this.visitor);
    }
    public IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
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
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.visitor);
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
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);

    public TField Sum<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TField Max<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TField Min<TField>(Expression<Func<T, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);

    public T First()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        T result = default;
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.To<T>(this.connection, readerFields);
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);

        var cmd = this.connection.CreateCommand();
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
            result = reader.To<T>(this.connection, readerFields);
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public List<T> ToList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);

        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new List<T>();
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        while (reader.Read())
        {
            result.Add(reader.To<T>(this.connection, readerFields));
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);

        var cmd = this.connection.CreateCommand();
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
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.To<T>(this.connection, readerFields));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public IPagedList<T> ToPageList(int pageIndex, int pageSize)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);

        var command = this.connection.CreateCommand();
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
            result.Items.Add(reader.To<T>(this.connection, readerFields));
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(behavior);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public async Task<IPagedList<T>> ToPageListAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.Page(pageIndex, pageSize);
        var sql = this.visitor.BuildSql(defaultExpr, out var dbParameters, out var readerFields);

        var cmd = this.connection.CreateCommand();
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
            result.Items.Add(reader.To<T>(this.connection, readerFields));
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader, this.connection);
        }
        return result;
    }
    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector) where TKey : notnull
        => this.ToList().ToDictionary(keySelector, valueSelector);
    public async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        var list = await this.ToListAsync(cancellationToken);
        return list.ToDictionary(keySelector, valueSelector);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        return this.visitor.BuildSql(defaultExpr, out dbParameters, out _);
    }

    private TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out _);
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        object result = null;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read()) result = reader.GetValue(0);
        reader.Close();
        reader.Dispose();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
    private async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
    {
        this.visitor.Select(sqlFormat, fieldExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out _);
        var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await this.connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
            result = reader.GetValue(0);
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}