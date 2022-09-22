using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ISqlExpression<T>
{
    ISqlExpression<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    ISqlExpression<T> InnerJoin(Expression<Func<T, bool>> predicate);
    ISqlExpression<T> LeftJoin(Expression<Func<T, bool>> predicate);
    ISqlExpression<T> RightJoin(Expression<Func<T, bool>> predicate);
    ISqlExpression<T> Where(Expression<Func<T, bool>> predicate);
    ISqlExpression<T> Where(bool condition, Expression<Func<T, bool>> predicate);
    ISqlExpression<T> Include<TTarget>(Expression<Func<T, TTarget>> memberSelector);

    IGroupBySqlExpression<TTarget> GroupBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    ISqlExpression<T> OrderBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    ISqlExpression<T> OrderByDescending<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    string ToSql();
    T First();
    Task<T> FirstAsync(CancellationToken cancellationToken = default);
    List<T> ToList();
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);
    Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector);
    Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector, CancellationToken cancellationToken = default);
    long Count();
    Task<long> CountAsync(CancellationToken cancellationToken = default);
    bool Exists(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
public interface IGroupBySqlExpression<TGroupKey>
{
    TTarget Max<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldExpr);
    TTarget Min<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldExpr);
    TTarget Average<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldExpr);
    int Count<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldExpr);
    ISqlExpression<TTarget> Select<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldsExpr);
    List<TGroupKey> ToList();
    IGroupBySqlExpression<TGroupKey> Having(Expression<Func<TGroupKey, bool>> predicate);
    ISqlExpression<TGroupKey> OrderBy<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldsExpr);
    ISqlExpression<TGroupKey> OrderByDescending<TTarget>(Expression<Func<TGroupKey, TTarget>> fieldsExpr);
    string ToSql();
}
