using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class FromCommand : IFromCommandBase
{
    #region Properties
    public Type EntityType { get; set; }
    public DbContext DbContext { get; set; }
    public IQueryVisitor Visitor { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public IFromCommand<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public int Execute()
    {
        return this.DbContext.Execute(f =>
        {
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        });
    }
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand()
    {
        var result = this.Visitor.CreateMultipleCommand();
        this.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        this.DbContext.Dispose();
        this.DbContext = null;
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public async ValueTask DisposeAsync()
    {
        await this.DbContext.DisposeAsync();
        this.Visitor.Dispose();
    }
    #endregion
}
public class FromCommand<T> : IFromCommand<T>
{
    #region Properties
    public Type EntityType { get; set; }
    public DbContext DbContext { get; set; }
    public IQueryVisitor Visitor { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public FromCommand(DbContext dbContext, IQueryVisitor visitor)
    {
        this.DbContext = dbContext;
        this.Visitor = visitor;
    }
    #endregion

    #region Union/UnionAll
    public IFromCommand<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this.Visitor);
            subQuery.Visitor.Dispose();
        }

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IFromCommand<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
        {
            query.Visitor.CopyTo(this.Visitor);
            query.Visitor.Dispose();
        }

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IFromCommand<T> UnionAll(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true, subQuery);
        sql += " UNION ALL" + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this.Visitor);
            subQuery.Visitor.Dispose();
        }
        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    public IFromCommand<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var sql = this.Visitor.BuildSql(out var readerFields, false, true);
        this.Visitor.Clear(true);
        var tableSegment = this.Visitor.WithTable(typeof(T), sql, readerFields, true);
        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        sql += " UNION ALL" + Environment.NewLine + query.Visitor.BuildSql(out _, false, true);
        if (!this.Visitor.Equals(query.Visitor))
        {
            query.Visitor.CopyTo(this.Visitor);
            query.Visitor.Dispose();
        }

        this.Visitor.Union(tableSegment, sql);
        return this;
    }
    #endregion

    #region Join   
    public IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this.Visitor);
            subQuery.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("INNER JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this.Visitor);
            subQuery.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var sql = subQuery.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this.Visitor);
            subQuery.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, subQuery);
        this.Visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
        {
            query.Visitor.CopyTo(this.Visitor);
            query.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("INNER JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
        {
            query.Visitor.CopyTo(this.Visitor);
            query.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("LEFT JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    public IFromCommand<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        var fromQuery = new FromQuery(this.DbContext, this.Visitor);
        var query = subQuery.Invoke(fromQuery);
        var sql = query.Visitor.BuildSql(out var readerFields, false);
        if (!this.Visitor.Equals(query.Visitor))
        {
            query.Visitor.CopyTo(this.Visitor);
            query.Visitor.Dispose();
        }
        var tableSegment = this.Visitor.WithTable(typeof(TOther), sql, readerFields, false, query);
        this.Visitor.Join("RIGHT JOIN", tableSegment, joinOn);
        return this.OrmProvider.NewFromCommand<T, TOther>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Where/And
    public IFromCommand<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IFromCommand<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return new GroupingQuery<T, TGrouping>(this.DbContext, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
         => this.OrderBy(true, fieldsExpr);
    public IFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    public IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewFromCommand<TTarget>(this.DbContext, this.Visitor);
    }
    #endregion

    #region Execute
    public int Execute()
    {
        throw new NotImplementedException();
    }
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region ToMultipleCommand
    public MultipleCommand ToMultipleCommand()
    {
        var result = this.Visitor.CreateMultipleCommand();
        this.Dispose();
        return result;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        this.DbContext.Dispose();
        this.DbContext = null;
        this.Visitor.Dispose();
        this.Visitor = null;
    }
    public async ValueTask DisposeAsync()
    {
        await this.DbContext.DisposeAsync();
        this.Visitor.Dispose();
    }
    #endregion
}
