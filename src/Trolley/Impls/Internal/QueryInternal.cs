using System;
using System.Collections;
using System.Linq.Expressions;

namespace Trolley;

public class QueryInternal : DialectProvider
{
    private bool isDisposed = false;
    #region Properties
    public IQueryVisitor Visitor { get; set; }
    public virtual bool IsCteTable => false;
    public virtual IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Union/UnionAll
    protected void UnionInternal<T>(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), subQuery);
    }
    protected void UnionInternal<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));

        this.Visitor.Union(" UNION", typeof(T), subQueryExpr);
    }
    protected void UnionAllInternal<T>(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
    }
    protected void UnionAllInternal<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));

        this.Visitor.Union(" UNION ALL", typeof(T), subQueryExpr);
    }
    protected void UnionRecursiveInternal<T>(Expression<Func<IFromQuery, ICteQuery<T>, IQuery<T>>> selfSubQueryExpr)
    {
        if (selfSubQueryExpr == null)
            throw new ArgumentNullException(nameof(selfSubQueryExpr));

        this.Visitor.UnionRecursive(" UNION", typeof(T), selfSubQueryExpr);
    }
    protected void UnionAllRecursiveInternal<T>(Expression<Func<IFromQuery, ICteQuery<T>, IQuery<T>>> selfSubQueryExpr)
    {
        if (selfSubQueryExpr == null)
            throw new ArgumentNullException(nameof(selfSubQueryExpr));

        this.Visitor.UnionRecursive(" UNION ALL", typeof(T), selfSubQueryExpr);
    }
    #endregion

    #region WithTable
    protected void WithQueryInternal<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.UseQuery(typeof(TOther), subQuery, false);
    }
    protected void WithQueryInternal<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));

        this.Visitor.UseNewQuery(typeof(TOther), subQueryExpr, false);
    }
    #endregion

    #region InnerJoin
    protected void InnerJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
    }
    protected void InnerJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", newEntityType, joinOn);
    }
    protected void InnerJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void InnerJoinInternal<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression joinOn)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQueryExpr, joinOn);
    }
    #endregion

    #region LeftJoin
    protected void LeftJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
    }
    protected void LeftJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", newEntityType, joinOn);
    }
    protected void LeftJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void LeftJoinInternal<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression joinOn)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQueryExpr, joinOn);
    }
    #endregion

    #region RightJoin
    protected void RightJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
    }
    protected void RightJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", newEntityType, joinOn);
    }
    protected void RightJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void RightJoinInternal<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression joinOn)
    {
        if (subQueryExpr == null)
            throw new ArgumentNullException(nameof(subQueryExpr));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQueryExpr, joinOn);
    }
    #endregion

    #region Include/IncludeMany
    protected bool IncludeInternal<TMember>(Expression memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        return this.Visitor.Include(memberSelector);
    }
    protected void IncludeManyInternal<TElement>(Expression memberSelector, Expression filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
    }
    #endregion

    #region And
    protected void AndByInternal(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.AndBy(whereObj);
    }
    protected void AndByIdInternal(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.AndById(whereKey);
    }
    protected void AndByIdsInternal(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.AndByIds(whereKeys);
    }
    protected void AndInternal(Expression predicate)
    {
        if (predicate == null) return;
        this.Visitor.And(predicate);
    }
    protected void AndInternal(bool condition, Expression ifPredicate, Expression elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
    }
    #endregion

    #region Or
    protected void OrByInternal(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.Visitor.OrBy(whereObj);
    }
    protected void OrByIdInternal(object whereKey)
    {
        if (whereKey == null)
            throw new ArgumentNullException(nameof(whereKey));
        this.Visitor.OrById(whereKey);
    }
    protected void OrByIdsInternal(IEnumerable whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));
        this.Visitor.OrByIds(whereKeys);
    }
    protected void OrInternal(Expression predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        this.Visitor.Or(predicate);
    }
    protected void OrInternal(bool condition, Expression ifPredicate, Expression elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Or(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Or(elsePredicate);
    }
    #endregion

    #region GroupBy
    protected void GroupByInternal(Expression groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));
        this.Visitor.GroupBy(groupingExpr);
    }
    #endregion

    #region OrderBy/OrderByDescending
    protected void OrderByInternal(bool condition, Expression fieldsExpr)
    {
        if (!condition) return;
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        this.Visitor.OrderBy("ASC", fieldsExpr);
    }
    protected void OrderByDescendingInternal(bool condition, Expression fieldsExpr)
    {
        if (!condition) return;
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        this.Visitor.OrderBy("DESC", fieldsExpr);
    }
    protected void OrderByDynamic(bool isAscending, Expression fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));
        this.Visitor.OrderBy(isAscending ? "ASC" : "DESC", fieldsExpr);
    }
    #endregion

    #region Having
    protected void HavingInternal(bool condition, Expression predicate)
    {
        if (!condition) return;
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
        this.Visitor.Having(predicate);
    }
    #endregion

    #region Skip/Take/Page
    protected void SkipInternal(int offset) => this.Visitor.Skip(offset);
    protected void TakeInternal(int limit) => this.Visitor.Take(limit);
    protected void PageInternal(int pageNumber, int pageSize)
        => this.Visitor.Page(pageNumber, pageSize);
    #endregion

    #region Select
    protected void SelectRawInternal(Type targetType, string rawFields)
    {
        if (string.IsNullOrEmpty(rawFields))
            throw new ArgumentNullException(nameof(rawFields));

        this.Visitor.SelectRaw(targetType, rawFields);
    }
    protected void SelectInternal(Expression fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(fieldsExpr);
    }
    #endregion

    public void Dispose()
    {
        if (this.isDisposed)
            return;
        this.Visitor.Dispose();
        this.DbContext = null;
        this.isDisposed = true;
    }
}