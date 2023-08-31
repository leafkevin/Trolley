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
    #region Fields
    private readonly IQueryVisitor visitor;
    #endregion

    #region Constructor
    public QueryAnonymousObject(IQueryVisitor visitor) => this.visitor = visitor;
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
    #endregion
}
class QueryBase : IQueryBase
{
    #region Fields
    protected string dbKey;
    protected TheaConnection connection;
    protected IDbTransaction transaction;
    protected IQueryVisitor visitor;
    protected int unionIndex;
    protected int withIndex;
    #endregion

    #region Count
    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    #endregion

    #region ToSql
    public virtual string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(out dbParameters, out _);
    #endregion

    #region QueryFirstValue
    protected TTarget QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
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
    protected async Task<TTarget> QueryFirstValueAsync<TTarget>(string sqlFormat, Expression fieldExpr = null, CancellationToken cancellationToken = default)
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
    #endregion
}
class Query<T> : QueryBase, IQuery<T>
{
    #region Fields
    private int? offset;
    private int pageIndex;
    private int pageSize;
    #endregion

    #region Constructor
    public Query(TheaConnection connection, IDbTransaction transaction, IQueryVisitor visitor, int withIndex = 0)
    {
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        this.dbKey = connection.DbKey;
        this.withIndex = withIndex;
    }
    #endregion

    #region Union/UnionAll
    public IQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.unionIndex++;
        var tableAsStar = (char)('a' + this.unionIndex);
        var newVisitor = this.visitor.Clone(tableAsStar, $"p{this.unionIndex}u");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true);
        this.visitor.Union(sql, readerFields, dbParameters);
        return this;
    }
    public IQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var tableAlias = (char)('a' + this.unionIndex);
        this.unionIndex++;
        var nextTableAlias = (char)('a' + this.unionIndex);
        var newVisitor = this.visitor.Clone(nextTableAlias, $"p{this.unionIndex}u");
        subQuery.Invoke(new FromQuery(newVisitor));
        var sql = " UNION ALL" + Environment.NewLine + newVisitor.BuildSql(out var dbParameters, out var readerFields, true, nextTableAlias);
        this.visitor.Union(sql, readerFields, dbParameters, tableAlias);
        return this;
    }
    #endregion

    #region CTE NextWith/NextWithRecursive
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

        this.withIndex++;
        var tableAsStar = (char)('a' + this.withIndex);
        var newVisitor = this.visitor.Clone(tableAsStar, $"p{this.withIndex}w");
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

    #region Where/And
    public IQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        return this;
    }
    public IQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.visitor.And(predicate);
        return this;
    }
    public IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
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
    public IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
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
    public IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
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
    #endregion

    #region Distinct
    public IQuery<T> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
            this.pageIndex = (int)Math.Ceiling((double)offset / this.pageSize);
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
            this.pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.visitor.Page(pageIndex, pageSize);
        return this;
    }
    #endregion

    #region Aggregate
    #region Count
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
    #endregion

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
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public T First()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);
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
            if (entityType.IsEntityType(out _))
                result = reader.To<T>(this.connection.DbKey, this.visitor.OrmProvider, readerFields);
            else result = reader.To<T>();
        }
        reader.Close();
        reader.Dispose();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        command.Dispose();
        return result;
    }
    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);

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
            if (entityType.IsEntityType(out _))
                result = reader.To<T>(this.dbKey, this.visitor.OrmProvider, readerFields);
            else result = reader.To<T>();
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            this.visitor.SetIncludeValues(result, includeReader);
        }
        await command.DisposeAsync();
        return result;
    }
    public List<T> ToList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);

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
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Add(reader.To<T>(this.dbKey, this.visitor.OrmProvider, readerFields));
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
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);

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
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.To<T>(this.connection.DbKey, this.visitor.OrmProvider, readerFields));
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
    public IPagedList<T> ToPageList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);

        using var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.Transaction = this.transaction;

        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => command.Parameters.Add(f));

        var result = new PagedList<T> { Data = new List<T>() };
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = command.ExecuteReader(behavior);
        if (reader.Read())
            result.TotalCount = reader.To<int>();
        result.PageIndex = this.pageIndex;
        result.PageSize = this.pageSize;

        reader.NextResult();
        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<T>(this.dbKey, this.visitor.OrmProvider, readerFields));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Data.Add(reader.To<T>());
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
    public async Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        var sql = this.visitor.BuildSql(out var dbParameters, out var readerFields);

        using var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Transaction = this.transaction;
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => cmd.Parameters.Add(f));

        if (cmd is not DbCommand command)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var result = new PagedList<T> { Data = new List<T>() };
        await this.connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess;
        using var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        if (await reader.ReadAsync())
            result.TotalCount = reader.To<int>();
        result.PageIndex = this.pageIndex;
        result.PageSize = this.pageSize;

        await reader.NextResultAsync(cancellationToken);
        var entityType = typeof(T);
        if (entityType.IsEntityType(out _))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<T>(this.dbKey, this.visitor.OrmProvider, readerFields));
            }
        }
        else
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Data.Add(reader.To<T>());
            }
        }
        await reader.CloseAsync();
        await reader.DisposeAsync();

        if (this.visitor.BuildIncludeSql(result.Data, out sql))
        {
            command.CommandText = sql;
            command.Parameters.Clear();
            using var includeReader = await command.ExecuteReaderAsync(behavior, cancellationToken);
            this.visitor.SetIncludeValues(result.Data, includeReader);
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
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.visitor.SelectDefault(defaultExpr);
        return this.visitor.BuildSql(out dbParameters, out _);
    }
    #endregion
}