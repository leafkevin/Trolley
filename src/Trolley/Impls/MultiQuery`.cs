﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MultiQuery<T1, T2> : MultiQueryBase, IMultiQuery<T1, T2>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3> : MultiQueryBase, IMultiQuery<T1, T2, T3>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>(IQuery<TOther> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));

        this.Visitor.From(typeof(TOther), this.DbContext, subQuery);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn)
    {
        if (subQuery == null)
            throw new ArgumentNullException(nameof(subQuery));
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", typeof(TOther), this.DbContext, subQuery, joinOn);
        return this.OrmProvider.NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    public IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}
public class MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : MultiQueryBase, IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
    #region Constructor
    public MultiQuery(MultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(false, tableNames);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(false, tableNamePredicate);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(false, masterEntityType, tableNameGetter);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(false, field1Value, field2Value);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(false, fieldValue1, fieldValue2, fieldValue3);
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

    #region Join
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("INNER JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("LEFT JOIN", joinOn);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.Visitor.Join("RIGHT JOIN", joinOn);
        return this;
    }
    #endregion

    #region Include
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        var isIncludeMany = this.Visitor.Include(memberSelector);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null)
    {
        if (memberSelector == null)
            throw new ArgumentNullException(nameof(memberSelector));

        this.Visitor.Include(memberSelector, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElment>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion

    #region Where/And
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.Where(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.Where(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.Where(elsePredicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        this.Visitor.And(predicate);
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null)
    {
        if (condition)
        {
            if (ifPredicate == null)
                throw new ArgumentNullException(nameof(ifPredicate));
            this.Visitor.And(ifPredicate);
        }
        else if (elsePredicate != null) this.Visitor.And(elsePredicate);
        return this;
    }
    #endregion

    #region GroupBy
    public IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr)
    {
        if (groupingExpr == null)
            throw new ArgumentNullException(nameof(groupingExpr));

        this.Visitor.GroupBy(groupingExpr);
        return this.OrmProvider.NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region OrderBy
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderBy(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("ASC", fieldsExpr);
        }
        return this;
    }
    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
        => this.OrderByDescending(true, fieldsExpr);

    public IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr)
    {
        if (condition)
        {
            if (fieldsExpr == null)
                throw new ArgumentNullException(nameof(fieldsExpr));
            this.Visitor.OrderBy("DESC", fieldsExpr);
        }
        return this;
    }
    #endregion

    #region Select 
    public IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr)
    {
        if (fieldsExpr == null)
            throw new ArgumentNullException(nameof(fieldsExpr));

        this.Visitor.Select(null, fieldsExpr);
        return this.OrmProvider.NewMultiQuery<TTarget>(this.MultipleQuery, this.Visitor);
    }
    #endregion

    #region Count
    public IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<int>("COUNT(DISTINCT {0})", fieldExpr);
    }
    public IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT({0})", fieldExpr);
    }
    public IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<long>("COUNT(DISTINCT {0})", fieldExpr);
    }
    #endregion

    #region Aggregate
    public IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("SUM({0})", fieldExpr);
    }
    public IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("AVG({0})", fieldExpr);
    }
    public IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MAX({0})", fieldExpr);
    }
    public IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        return this.QueryFirstValue<TField>("MIN({0})", fieldExpr);
    }
    #endregion 
}