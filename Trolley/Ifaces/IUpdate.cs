using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

namespace Trolley;

public interface IUpdate<TEntity>
{
    IUpdate<TEntity> RawSql(string rawSql);
    IUpdate<TEntity> SetByKey<TUpdateObject>(TUpdateObject updateObjs, int bulkCount = 500);
    IUpdate<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);

    IUpdate<TEntity, T> From<T>(Expression<Func<TEntity, T, bool>> joinOn);
    IUpdate<TEntity, T1, T2> From<T1, T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);

    IUpdate<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn);
    IUpdate<TEntity, T1, T2> InnerJoin<T1, T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3> InnerJoin<T1, T2, T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3, T4> InnerJoin<T1, T2, T3, T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdate<TEntity, T1, T2, T3, T4, T5> InnerJoin<T1, T2, T3, T4, T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);

    IUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    IUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<TEntity, T>
{
    IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T, TSetObject>> setExpr);
}
public interface IUpdate<T, T1, T2>
{
    IUpdate<T, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, TOther, bool>> predicate);
    IUpdate<T, T1, T2> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1, T2> Set<TSetObject>(Expression<Func<T, T1, T2, TSetObject>> setExpr);
    IUpdate<T, T1, T2> Where(Expression<Func<T, T1, T2, bool>> predicate);
    IUpdate<T, T1, T2> Where(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<T, T1, T2, T3>
{
    IUpdate<T, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, TOther, bool>> predicate);
    IUpdate<T, T1, T2, T3> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1, T2, T3> Set<TSetObject>(Expression<Func<T, T1, T2, T3, TSetObject>> setExpr);
    IUpdate<T, T1, T2, T3> Where(Expression<Func<T, T1, T2, T3, bool>> predicate);
    IUpdate<T, T1, T2, T3> Where(bool condition, Expression<Func<T, T1, T2, T3, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<T, T1, T2, T3, T4>
{
    IUpdate<T, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, T4, TOther, bool>> predicate);
    IUpdate<T, T1, T2, T3, T4> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, TSetObject>> setExpr);
    IUpdate<T, T1, T2, T3, T4> Where(Expression<Func<T, T1, T2, T3, T4, bool>> predicate);
    IUpdate<T, T1, T2, T3, T4> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<T, T1, T2, T3, T4, T5>
{
    IUpdate<T, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, T4, T5, TOther, bool>> predicate);
    IUpdate<T, T1, T2, T3, T4, T5> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, T5, TSetObject>> setExpr);
    IUpdate<T, T1, T2, T3, T4, T5> Where(Expression<Func<T, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdate<T, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, T5, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<T, T1, T2, T3, T4, T5, T6>
{
    IUpdate<T, T1, T2, T3, T4, T5, T6> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1, T2, T3, T4, T5, T6> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, T5, T6, TSetObject>> setExpr);
    IUpdate<T, T1, T2, T3, T4, T5, T6> Where(Expression<Func<T, T1, T2, T3, T4, T5, T6, bool>> predicate);
    IUpdate<T, T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, T5, T6, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
