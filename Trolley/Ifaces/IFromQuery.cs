using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IFromQuery
{
    IFromQuery<T> From<T>(char tableStartAs = 'a');
    IFromQuery<T1, T2> From<T1, T2>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableStartAs = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableStartAs = 'a');
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T>
{
    IFromQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T> Where(Expression<Func<T, bool>> predicate = null);
    IFromQuery<T> And(bool condition, Expression<Func<T, bool>> predicate = null);
    IFromGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    IFromQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IFromQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    IFromQuery<T> Distinct();
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2>
{
    IFromQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate = null);
    IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    IFromQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    IFromQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3>
{
    IFromQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate = null);
    IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4>
{
    IFromQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4, T5>
{
    IFromQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Union<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UnionAll<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
}