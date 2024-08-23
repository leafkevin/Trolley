using System;
using System.Linq.Expressions;

namespace Trolley;

public class QueryInternal
{
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
    protected void UnionInternal<T>(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION", typeof(T), this.DbContext, subQuery);
    }
    protected void UnionAllInternal<T>(IQuery<T> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), subQuery);
    }
    protected void UnionAllInternal<T>(Func<IFromQuery, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.Union(" UNION ALL", typeof(T), this.DbContext, subQuery);
    }
    protected void UnionRecursiveInternal<T>(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var cteQuery = new CteQuery<T>(this.DbContext, this.Visitor);
        this.Visitor.UnionRecursive(" UNION", this.DbContext, cteQuery, subQuery);
    }
    protected void UnionAllRecursiveInternal<T>(Func<IFromQuery, IQuery<T>, IQuery<T>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        var cteQuery = new CteQuery<T>(this.DbContext, this.Visitor);
        this.Visitor.UnionRecursive(" UNION ALL", this.DbContext, cteQuery, subQuery);
    }
    #endregion

    #region WithTable
    protected void WithTableInternal<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
    }
    protected void WithTableInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
    }
    #endregion

    #region Join
    protected void InnerJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
    }
    protected void LeftJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
    }
    protected void RightJoinInternal(Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
    }
    protected void InnerJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", newEntityType, joinOn);
    }
    protected void LeftJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", newEntityType, joinOn);
    }
    protected void RightJoinInternal(Type newEntityType, Expression joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", newEntityType, joinOn);
    }
    protected void InnerJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void LeftJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void RightJoinInternal<TOther>(IQuery<TOther> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
    }
    protected void InnerJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
    }
    protected void LeftJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
    }
    protected void RightJoinInternal<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
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

    #region WhereInternal/AndInternal
    protected void WhereInternal(Expression predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
    }
    protected void WhereInternal(bool condition, Expression ifPredicate, Expression elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
    }
    protected void AndInternal(Expression predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
    }
    protected void AndInternal(bool condition, Expression ifPredicate, Expression elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.Visitor.And(ifPredicate);
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
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
