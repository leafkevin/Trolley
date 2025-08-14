using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MultiQuery<T1, T2> : MultiQueryBase, IMultiQuery<T1, T2>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3> : MultiQueryBase, IMultiQuery<T1, T2, T3>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region WithTable
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>()
    {
        this.Visitor.AddTable(typeof(TOther));
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region WithQuery
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(IQuery<TOther> subQuery)
    {
        base.WithQueryInternal(subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr)
    {
        base.WithQueryInternal(subQueryExpr);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.InnerJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.LeftJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        base.RightJoinInternal(subQueryExpr, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    #region Constructor
    public MultiQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(false, tableSchema);
        return this;
    }
    #endregion

    #region InnerJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.InnerJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region LeftJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.LeftJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region RightJoin
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        base.RightJoinInternal(joinOn);
        return this;
    }
    #endregion

    #region Include
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector)
    {
        var isIncludeMany = base.IncludeInternal<TMember>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null)
    {
        base.IncludeManyInternal<TElement>(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        base.WhereInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        base.WhereInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        return this.Where(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region And
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        base.AndInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        base.AndInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        return this.And(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region Or
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        base.OrInternal(predicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        base.OrInternal(condition, ifPredicate, elsePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer)
    {
        var builder = new PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        return this.Or(predicateInitializer.Invoke(builder));
    }
    #endregion

    #region GroupBy
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr)
    {
        base.GroupByInternal(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        base.OrderByInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        base.OrderByDescendingInternal(condition, fieldsExpr);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression> fieldsGetter)
    {
        var builder = new OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>();
        var fieldsExpr = fieldsGetter.Invoke(builder);
        base.OrderByDynamic(builder.IsAscending, fieldsExpr);
        return this;
    }
    #endregion

    #region Skip/Take/Page
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Skip(int offset)
    {
        base.SkipInternal(offset);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Take(int limit)
    {
        base.TakeInternal(limit);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Page(int pageNumber, int pageSize)
    {
        base.PageInternal(pageNumber, pageSize);
        return this;
    }
    #endregion

    #region Select 
    public virtual IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr)
    {
        base.SelectInternal(fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public virtual IMultiQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> specialMemberSelector = null)
    {
        this.Visitor.SelectTo(typeof(TTarget), specialMemberSelector);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public virtual IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.CountInternal(fieldExpr);
    public virtual IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.CountDistinctInternal(fieldExpr);
    public virtual IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.LongCountInternal(fieldExpr);
    public virtual IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.LongCountDistinctInternal(fieldExpr);
    #endregion

    #region Aggregate
    public virtual IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.SumInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.AvgInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.MaxInternal<TField>(fieldExpr);
    public virtual IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
        => base.MinInternal<TField>(fieldExpr);
    #endregion
}