using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public class MultiQueryBase : IMultiQueryBase
{
    #region Properties
    public IQueryVisitor Visitor { get; set; }
    public MultipleQuery MultipleQuery { get; set; }
    public DbContext DbContext => this.MultipleQuery.DbContext;
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Constructor
    public MultiQueryBase(MultipleQuery multipleQuery, IQueryVisitor visitor)
    {
        this.MultipleQuery = multipleQuery;
        this.Visitor = visitor;
    }
    #endregion

    #region Select
    public IMultiQuery<TTarget> Select<TTarget>(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count() => this.QueryFirstValue<int>("COUNT(1)");
    public IMultipleQuery LongCount() => this.QueryFirstValue<long>("COUNT(1)");
    #endregion    

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        return this.Visitor.BuildSql(out _);
    }
    #endregion

    #region QueryFirstValue
    protected IMultipleQuery QueryFirstValue<TTarget>(string sqlFormat, Expression fieldExpr = null)
    {
        this.Visitor.Select(sqlFormat, fieldExpr);
        var sql = this.Visitor.BuildSql(out _);
        Func<IDataReader, object> readerGetter = reader => reader.To<TTarget>(this.OrmProvider);
        this.MultipleQuery.AddReader(typeof(TTarget), sql, readerGetter);
        return this.MultipleQuery;
    }
    #endregion
}
public class MultiQuery<T> : MultiQueryBase, IMultiQuery<T>
{
    #region Fields
    private int? offset;
    private int pageIndex;
    private int pageSize;
    #endregion

    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Union/UnionAll
    public IMultiQuery<T> Union(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), subQuery);
        return this;
    }
    public IMultiQuery<T> Union(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), this.DbContext, subQuery);
        return this;
    }
    public IMultiQuery<T> UnionAll(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
        return this;
    }
    public IMultiQuery<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), this.DbContext, subQuery);
        return this;
    }
    #endregion    

    #region WithTable
    public IMultiQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return new MultiQuery<T, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector);
        return new MultiIncludableQuery<T, TMember>(this.MultipleQuery, this.Visitor);
    }
    public IMultiIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, true, filter);
        return new MultiIncludableQuery<T, TElment>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T> And(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy/OrderBy
    public IMultiGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return new MultiGroupingQuery<T, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public IMultiQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("ASC", fieldsExpr);
        return this;
    }
    public IMultiQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public IMultiQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        if (condition)
            this.Visitor.OrderBy("DESC", fieldsExpr);
        return this;
    }
    #endregion

    #region Select
    public IMultiQuery<T> Select()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.Select(null, defaultExpr);
        return this;
    }
    //TODO:
    //public IMultiQuery<TTarget> Select<TTarget>(TTarget parameters)
    //{
    //    if (parameters == null)
    //        throw new ArgumentNullException(nameof(parameters));
    //    //TODO:
    //    this.visitor.Select(fields, null, true);
    //    var fromQuery = new FromQuery<TTarget>(this.connection, this.transaction,this.ormProvider, this.mapProvider, this.visitor, this.insertType);
    //    //this.visitor.Select()
    //    return fromQuery;
    //}
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return new MultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Distinct
    public IMultiQuery<T> Distinct()
    {
        this.Visitor.Distinct();
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public IMultiQuery<T> Skip(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
        {
            var pageIndex = (int)Math.Ceiling((double)this.offset.Value / this.pageSize);
            this.Visitor.Page(pageIndex, this.pageSize);
        }
        else this.Visitor.Skip(offset);
        return this;
    }
    public IMultiQuery<T> Take(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
        {
            var pageIndex = (int)Math.Ceiling((double)this.offset.Value / limit);
            this.Visitor.Page(pageIndex, limit);
        }
        else this.Visitor.Take(limit);
        return this;
    }
    public IMultiQuery<T> Page(int pageIndex, int pageSize)
    {
        this.pageIndex = pageIndex;
        this.pageSize = pageSize;
        this.Visitor.Page(pageIndex, pageSize);
        return this;
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    public IMultipleQuery First() => this.QueryResult();
    public IMultipleQuery ToList() => this.QueryResult();
    public IMultipleQuery ToPageList() => this.QueryResult();
    public IMultipleQuery ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.QueryResult();
    }
    #endregion

    #region QueryResult
    private IMultipleQuery QueryResult()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        this.Visitor.SelectDefault(defaultExpr);
        var sql = this.Visitor.BuildSql(out var readerFields);
        var targetType = typeof(T);
        Func<IDataReader, object> readerGetter = null;
        if (targetType.IsEntityType(out _))
            readerGetter = reader => reader.To<T>(this.DbContext, readerFields);
        else readerGetter = reader => reader.To<T>(this.OrmProvider);
        IQueryVisitor queryVisitor = null;
        if (this.Visitor.HasIncludeTables())
            queryVisitor = this.Visitor;
        this.MultipleQuery.AddReader(targetType, sql, readerGetter, queryVisitor, this.pageIndex, this.pageSize);
        return this.MultipleQuery;
    }
    #endregion
}
