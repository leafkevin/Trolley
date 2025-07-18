using System;
using System.Linq.Expressions;

namespace Trolley;

public class QueryInternal
{
    #region Fields
    protected int? offset;
    protected int pageNumber;
    protected int pageSize;
    #endregion

    #region Properties
    public DbContext DbContext { get; set; }
    public IQueryVisitor Visitor { get; set; }
    public virtual IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    #endregion

    #region Union/UnionAll
    protected void UnionInternal<T>(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), subQuery);
    }
    protected void UnionInternal<T>(Func<IFromQuery, IQuery<T>> subQueryGetter)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));

        this.Visitor.Union(" UNION", typeof(T), subQueryGetter);
    }
    protected void UnionAllInternal<T>(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
    }
    protected void UnionAllInternal<T>(Func<IFromQuery, IQuery<T>> subQueryGetter)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));

        this.Visitor.Union(" UNION ALL", typeof(T), subQueryGetter);
    }
    protected void UnionRecursiveInternal<T>(Func<IFromQuery, IQuery<T>, IQuery<T>> selfSubQueryExpr)
    {
        if (selfSubQueryExpr == null)
            throw new ArgumentNullException(nameof(selfSubQueryExpr));

        var cteQuery = new CteQuery<T>(this.DbContext, this.Visitor);
        this.Visitor.UnionRecursive(" UNION", cteQuery, selfSubQueryExpr);
    }
    protected void UnionAllRecursiveInternal<T>(Func<IFromQuery, IQuery<T>, IQuery<T>> selfSubQueryExpr)
    {
        if (selfSubQueryExpr == null)
            throw new ArgumentNullException(nameof(selfSubQueryExpr));

        var cteQuery = new CteQuery<T>(this.DbContext, this.Visitor);
        this.Visitor.UnionRecursive(" UNION ALL", cteQuery, selfSubQueryExpr);
    }
    #endregion

    #region WithQuery
    protected void WithQueryInternal<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.UseQuery(typeof(TOther), subQuery, true, false);
    }
    protected void WithQueryInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));

        this.Visitor.UseNewQuery(typeof(TOther), subQueryGetter, false);
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
    protected void InnerJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression joinOn)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQueryGetter, joinOn);
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
    protected void LeftJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression joinOn)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQueryGetter, joinOn);
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
    protected void RightJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQueryGetter, Expression joinOn)
    {
        if (subQueryGetter == null)
            throw new ArgumentNullException(nameof(subQueryGetter));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQueryGetter, joinOn);
    }
    #endregion

    #region Include/IncludeMany
    protected bool IncludeInternal<TMember>(Expression memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        return this.Visitor.Include(memberSelector);
    }
    protected void IncludeManyInternal<TElment>(Expression memberSelector, Expression filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
    }
    #endregion

    #region Where
    protected void WhereInternal(Expression predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
    }
    protected void WhereInternal(bool condition, Expression ifPredicate, Expression elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
    }
    #endregion

    #region And
    protected void AndInternal(Expression predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

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
    protected void SkipInternal(int offset)
    {
        this.offset = offset;
        if (this.pageSize > 0)
        {
            this.pageNumber = (int)Math.Ceiling((double)offset / this.pageSize) + 1;
            this.Visitor.Page(this.pageNumber, this.pageSize);
        }
        else this.Visitor.Skip(offset);
    }
    protected void TakeInternal(int limit)
    {
        this.pageSize = limit;
        if (this.offset.HasValue)
        {
            this.pageNumber = (int)Math.Ceiling((double)offset / this.pageSize) + 1;
            this.Visitor.Page(this.pageNumber, this.pageSize);
        }
        else this.Visitor.Take(limit);
    }
    protected void PageInternal(int pageNumber, int pageSize)
    {
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.Visitor.Page(pageNumber, pageSize);
    }
    #endregion

    #region Select
    protected void SelectInternal(string fields = "*")
    {
        if (string.IsNullOrEmpty(fields))
            throw new ArgumentNullException(nameof(fields));

        this.Visitor.Select(fields, null);
    }
    protected void SelectInternal(Expression fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
    }
    #endregion  
}
