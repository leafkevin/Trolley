//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;

//namespace Trolley;

///// <summary>
///// 单表查询，可以带导航实体
///// </summary>
///// <typeparam name="T"></typeparam>
//public interface IJoinQuery<T> : IQuery
//{
//    IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector, Expression<Func<TMember, bool>> filter = null);
//    IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);

//    /// <summary>
//    /// 添加子查询表
//    /// </summary>
//    /// <typeparam name="TOther"></typeparam>
//    /// <param name="subQuery"></param>
//    /// <returns></returns>
//    IJoinQuery<T, TOther> WithTable<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery);
//    /// <summary>
//    /// 单表Include表关联
//    /// </summary>
//    /// <param name="joinOn"></param>
//    /// <returns></returns>
//    IJoinQuery<T> InnerJoin(Expression<Func<T, bool>> joinOn);
//    IJoinQuery<T> LeftJoin(Expression<Func<T, bool>> joinOn);
//    IJoinQuery<T> RightJoin(Expression<Func<T, bool>> joinOn);
//    IJoinQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
//    IJoinQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
//    IJoinQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);

//    IJoinQuery<T> Where(Expression<Func<T, bool>> predicate);
//    IJoinQuery<T> Where(bool condition, Expression<Func<T, bool>> predicate);

//    IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
//    IJoinQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
//    IJoinQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);

//    IQuery<T> Select();
//    IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
//    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
//    string ToSql();
//}
///// <summary>
///// 多表查询，也可以带导航实体
///// </summary>
///// <typeparam name="T"></typeparam>
///// <typeparam name="T1"></typeparam>
//public interface IJoinQuery<T, T1> : IQuery
//{
//    IIncludableQuery<T, T1, TMember> Include<TMember>(Expression<Func<T1, TMember>> memberSelector, Expression<Func<T1, TMember, bool>> filter = null);
//    IIncludableQuery<T, T1, TElment> IncludeMany<TElment>(Expression<Func<T1, IEnumerable<TElment>>> memberSelector, Expression<Func<T1, TElment, bool>> filter = null);

//    IJoinQuery<T, T1> InnerJoin(Expression<Func<T, T1, bool>> joinOn);
//    IJoinQuery<T, T1> LeftJoin(Expression<Func<T, T1, bool>> joinOn);
//    IJoinQuery<T, T1> RightJoin(Expression<Func<T, T1, bool>> joinOn);

//    IJoinQuery<T, T1, TOther> WithTable<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery);

//    IJoinQuery<T, T1, TOther> InnerJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, TOther> LeftJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, TOther> RightJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);

//    IJoinQuery<T, T1> Where(Expression<Func<T, T1, bool>> predicate);
//    IJoinQuery<T, T1> Where(bool condition, Expression<Func<T, T1, bool>> predicate);
//    IJoinQuery<T, T1> And(Expression<Func<T, T1, bool>> predicate);
//    IJoinQuery<T, T1> And(bool condition, Expression<Func<T, T1, bool>> predicate);
//    IJoinQuery<T, T1> Or(Expression<Func<T, T1, bool>> predicate);
//    IJoinQuery<T, T1> Or(bool condition, Expression<Func<T, T1, bool>> predicate);

//    IGroupingQuery<T, T1, TGrouping> GroupBy<TGrouping>(Expression<Func<T, T1, TGrouping>> groupingExpr);
//    IJoinQuery<T, T1> OrderBy<TFields>(Expression<Func<T, T1, TFields>> fieldsExpr);
//    IJoinQuery<T, T1> OrderByDescending<TFields>(Expression<Func<T, T1, TFields>> fieldsExpr);
//    IQuery<TTarget> Select<TTarget>(Expression<Func<T, T1, TTarget>> fieldsExpr);
//    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, T1, TTarget>> fieldsExpr);
//    string ToSql();
//}
//public interface IJoinQuery<T, T1, T2> : IQuery
//{
//    IJoinQuery<T, T1, T2> Include<TMember>(Expression<Func<T2, TMember>> member);
//    IJoinQuery<T, T1, T2> IncludeMany<TElment>(Expression<Func<T2, IEnumerable<TElment>>> member);
//    IJoinQuery<T, T1, T2> Include<TMember>(Expression<Func<T2, TMember>> member, Expression<Func<TMember, bool>> filter = null);
//    IJoinQuery<T, T1, T2> IncludeMany<TElment>(Expression<Func<T2, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);

//    IJoinQuery<T, T1, T2> InnerJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, T2> LeftJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, T2> RightJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);

//    IJoinQuery<T, T1, T2> Where(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Where(Expression<Func<IWhereSql, T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Where(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Where(bool condition, Expression<Func<IWhereSql, T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> And(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> And(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> And(Expression<Func<IFromQuery, T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> And(bool condition, Expression<Func<IWhereSql, T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Or(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Or(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Or(Expression<Func<IFromQuery, T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2> Or(bool condition, Expression<Func<IWhereSql, T, T1, T2, bool>> predicate);

//    IGroupingQuery<T, T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T, T1, T2, TGrouping>> groupingExpr);
//    IJoinQuery<T, T1, T2> OrderBy<TFields>(Expression<Func<T, T1, T2, TFields>> fieldsExpr);
//    IJoinQuery<T, T1, T2> OrderByDescending<TFields>(Expression<Func<T, T1, T2, TFields>> fieldsExpr);

//    IQuery<TTarget> Select<TTarget>(Expression<Func<T, T1, T2, TTarget>> fieldsExpr);
//    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, T1, T2, TTarget>> fieldsExpr);
//    string ToSql();
//}
//public interface IJoinQuery<T, T1, T2, T3> : IQuery
//{
//    IJoinQuery<T, T1, T2, T3> Include<TMember>(Expression<Func<T2, TMember>> member);
//    IJoinQuery<T, T1, T2, T3> IncludeMany<TElment>(Expression<Func<T2, IEnumerable<TElment>>> member);
//    IJoinQuery<T, T1, T2, T3> Include<TMember, TNavigation>(Expression<Func<T2, TMember>> member, Expression<Func<IIncluded<TMember>, TNavigation>> thenInclude);
//    IJoinQuery<T, T1, T2, T3> IncludeMany<TElment, TNavigation>(Expression<Func<T2, IEnumerable<TElment>>> member, Expression<Func<IIncluded<TElment>, TNavigation>> thenInclude);

//    IJoinQuery<T, T1, T2, T3> InnerJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, T2, T3> LeftJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);
//    IJoinQuery<T, T1, T2, T3> RightJoin(Expression<Func<T, T1, T2, bool>> joinOn);
//    IJoinQuery<T, T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQuery, Expression<Func<T, T1, T2, TOther, bool>> joinOn);

//    IJoinQuery<T, T1, T2, T3> Where(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2, T3> Where(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2, T3> And(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2, T3> And(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2, T3> Or(Expression<Func<T, T1, T2, bool>> predicate);
//    IJoinQuery<T, T1, T2, T3> Or(bool condition, Expression<Func<T, T1, T2, bool>> predicate);

//    IGroupingQuery<T, T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T, T1, T2, TGrouping>> groupingExpr);
//    IJoinQuery<T, T1, T2, T3> OrderBy<TFields>(Expression<Func<T, T1, T2, TFields>> fieldsExpr);
//    IJoinQuery<T, T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T, T1, T2, TFields>> fieldsExpr);

//    IQuery<TTarget> Select<TTarget>(Expression<Func<T, T1, T2, TTarget>> fieldsExpr);
//    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, T1, T2, TTarget>> fieldsExpr);
//    string ToSql();
//}
//public interface IMultiJoinQuery<T, T1> : IQuery
//{
//    IMultiJoinQuery<T, T1> Include<TMember>(Expression<Func<T1, TMember>> member);
//    IMultiJoinQuery<T, T1> IncludeMany<TElment>(Expression<Func<T1, IEnumerable<TElment>>> member);
//    IMultiJoinQuery<T, T1> Include<TMember, TNavigation>(Expression<Func<T1, TMember>> member, Expression<Func<IIncluded<TMember>, TNavigation>> thenInclude);
//    IMultiJoinQuery<T, T1> IncludeMany<TElment, TNavigation>(Expression<Func<T1, IEnumerable<TElment>>> member, Expression<Func<IIncluded<TElment>, TNavigation>> thenInclude);

//    IMultiJoinQuery<T, T1, TOther> InnerJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);
//    IMultiJoinQuery<T, T1, TOther> InnerJoin<TOther>(IMultiQuery<TOther> subQuery, Expression<Func<T, T1, TOther, bool>> joinOn);
//    IMultiJoinQuery<T, T1, TOther> LeftJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);
//    IMultiJoinQuery<T, T1, TOther> LeftJoin<TOther>(IMultiQuery<TOther> subQuery, Expression<Func<T, T1, TOther, bool>> joinOn);
//    IMultiJoinQuery<T, T1, TOther> RightJoin<TOther>(Expression<Func<T, T1, TOther, bool>> joinOn);
//    IMultiJoinQuery<T, T1, TOther> RightJoin<TOther>(IMultiQuery<TOther> subQuery, Expression<Func<T, T1, TOther, bool>> joinOn);

//    IMultiJoinQuery<T, T1> Where(Expression<Func<T, T1, bool>> predicate);
//    IMultiJoinQuery<T, T1> Where(bool condition, Expression<Func<T, T1, bool>> predicate);
//    IMultiJoinQuery<T, T1> And(Expression<Func<T, T1, bool>> predicate);
//    IMultiJoinQuery<T, T1> And(bool condition, Expression<Func<T, T1, bool>> predicate);
//    IMultiJoinQuery<T, T1> Or(Expression<Func<T, T1, bool>> predicate);
//    IMultiJoinQuery<T, T1> Or(bool condition, Expression<Func<T, T1, bool>> predicate);

//    IMultiGroupingQuery<T, T1, TGrouping> GroupBy<TGrouping>(Expression<Func<T, T1, TGrouping>> groupingExpr);
//    IMultiJoinQuery<T, T1> OrderBy<TFields>(Expression<Func<T, T1, TFields>> fieldsExpr);
//    IMultiJoinQuery<T, T1> OrderByDescending<TFields>(Expression<Func<T, T1, TFields>> fieldsExpr);

//    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T, T1, TTarget>> fieldsExpr);
//    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, T1, TTarget>> fieldsExpr);
//    string ToSql();
//}