using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Query<T1, T2> : IQuery<T1, T2>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2> Where(Expression<Func<IWhereSql, T1, T2, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2> Where(bool condition, Expression<Func<IWhereSql, T1, T2, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3> : IQuery<T1, T2, T3>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3> Where(Expression<Func<IWhereSql, T1, T2, T3, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4> : IQuery<T1, T2, T3, T4>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5> : IQuery<T1, T2, T3, T4, T5>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6> : IQuery<T1, T2, T3, T4, T5, T6>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7> : IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var fromQuery = new FromQuery($"p{this.withIndex++}w");
        var query = subQuery.Invoke(fromQuery);
        var sql = query.ToSql(out var dbDataParameters);
        this.visitor.WithTable(typeof(TOther), sql, dbDataParameters);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<IWhereSql, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}
class Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    private int withIndex = 0;
    private int unionIndex = 0;
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private readonly QueryVisitor visitor;

    public Query(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, QueryVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.visitor = visitor;
        visitor.From(typeof(T1), typeof(T2));
    }

    #region Include
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    public IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        this.visitor.Include(memberSelector, filter);
        return new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElment>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }
    #endregion

    #region Join
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        this.visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        this.visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        this.visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    #endregion

    #region Union
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Union<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UnionAll<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        var dbParameters = new List<IDbDataParameter>();
        var sql = this.ToSql(out var parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        var fromQuery = new FromQuery($"p{this.unionIndex++}u");
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL " + query.ToSql(out parameters);
        if (parameters != null && parameters.Count > 0)
            dbParameters.AddRange(parameters);

        this.visitor.Union(typeof(T1), sql, dbParameters);
        return this;
    }
    #endregion

    #region Where
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        this.visitor.Where(predicate);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr)
    {
        this.visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(this.visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        this.visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Distinct()
    {
        this.visitor.Distinct();
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Skip(int offset)
    {
        this.visitor.Skip(offset);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Take(int limit)
    {
        this.visitor.Take(limit);
        return this;
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> ToChunk(int size)
    {
        throw new NotImplementedException();
    }

    public IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr)
    {
        this.visitor.Select(null, fieldsExpr);
        return new Query<TTarget>(this.dbFactory, this.connection, this.transaction, this.visitor);
    }

    public int Count() => this.QueryFirstValue<int>("COUNT(1)");
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(*)", null, cancellationToken);
    public long LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(1)", null, cancellationToken);
    public int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    public async Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT({0})", fieldExpr, cancellationToken);
    public int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<int>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    public async Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT({0})", fieldExpr, cancellationToken);
    public long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    public async Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<long>("COUNT(DISTINCT {0})", fieldExpr, cancellationToken);
    public TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    public async Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("SUM({0})", fieldExpr, cancellationToken);
    public TTarget SumAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<TTarget>("SUM({0})", fieldExpr);
    public async Task<TTarget> SumAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TTarget>("SUM({0})", fieldExpr, cancellationToken);
    public TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    public async Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("AVG({0})", fieldExpr, cancellationToken);
    public TTarget AvgAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> AvgAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"AVG(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    public async Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MAX({0})", fieldExpr, cancellationToken);
    public TTarget MaxAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MaxAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MAX(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    public async Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
        => await this.QueryFirstValueAsync<TField>("MIN({0})", fieldExpr, cancellationToken);
    public TTarget MinAs<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return this.QueryFirstValue<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr);
    }
    public async Task<TTarget> MinAsAsync<TField, TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default)
    {
        var castTo = this.connection.OrmProvider.CastTo(typeof(TTarget));
        return await this.QueryFirstValueAsync<TTarget>($"MIN(CAST({{0}} AS {castTo}))", fieldExpr, cancellationToken);
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
        => this.visitor.BuildSql(null, out dbParameters, out _);
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

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        var reader = command.ExecuteReader(behavior);
        object result = null;
        while (reader.Read())
        {
            result = reader.GetValue(0);
        }
        while (reader.NextResult()) { }
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
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        object result = null;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result = reader.GetValue(0);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        if (result is DBNull) return default;
        return (TTarget)result;
    }
}