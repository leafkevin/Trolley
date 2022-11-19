using System;
using System.Linq.Expressions;

namespace Trolley;

public interface IUpdate<T>
{
    IUpdate<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> predicate);
    IUpdate<T> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T> Set<TSetObject>(Expression<Func<T, TSetObject>> setExpr);
    IUpdate<T> Where(Expression<Func<T, bool>> predicate);
    IUpdate<T> Where(bool condition, Expression<Func<T, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdate<T, T1>
{
    IUpdate<T, T1, TOther> InnerJoin<TOther>(Expression<Func<T, T1, TOther, bool>> predicate);
    IUpdate<T, T1> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IUpdate<T, T1> Set<TSetObject>(Expression<Func<T, T1, TSetObject>> setExpr);
    IUpdate<T, T1> Where(Expression<Func<T, T1, bool>> predicate);
    IUpdate<T, T1> Where(bool condition, Expression<Func<T, T1, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
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

public interface IMultiUpdate<T>
{
    IMultiUpdate<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> predicate);
    IMultiUpdate<T> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T> Set<TSetObject>(Expression<Func<T, TSetObject>> setExpr);
    IMultiUpdate<T> Where(Expression<Func<T, bool>> predicate);
    IMultiUpdate<T> Where(bool condition, Expression<Func<T, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1>
{
    IMultiUpdate<T, T1, TOther> InnerJoin<TOther>(Expression<Func<T, T1, TOther, bool>> predicate);
    IMultiUpdate<T, T1> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1> Set<TSetObject>(Expression<Func<T, T1, TSetObject>> setExpr);
    IMultiUpdate<T, T1> Where(Expression<Func<T, T1, bool>> predicate);
    IMultiUpdate<T, T1> Where(bool condition, Expression<Func<T, T1, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1, T2>
{
    IMultiUpdate<T, T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, TOther, bool>> predicate);
    IMultiUpdate<T, T1, T2> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1, T2> Set<TSetObject>(Expression<Func<T, T1, T2, TSetObject>> setExpr);
    IMultiUpdate<T, T1, T2> Where(Expression<Func<T, T1, T2, bool>> predicate);
    IMultiUpdate<T, T1, T2> Where(bool condition, Expression<Func<T, T1, T2, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1, T2, T3>
{
    IMultiUpdate<T, T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, TOther, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1, T2, T3> Set<TSetObject>(Expression<Func<T, T1, T2, T3, TSetObject>> setExpr);
    IMultiUpdate<T, T1, T2, T3> Where(Expression<Func<T, T1, T2, T3, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3> Where(bool condition, Expression<Func<T, T1, T2, T3, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1, T2, T3, T4>
{
    IMultiUpdate<T, T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, T4, TOther, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3, T4> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, TSetObject>> setExpr);
    IMultiUpdate<T, T1, T2, T3, T4> Where(Expression<Func<T, T1, T2, T3, T4, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3, T4> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1, T2, T3, T4, T5>
{
    IMultiUpdate<T, T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T, T1, T2, T3, T4, T5, TOther, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3, T4, T5> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, T5, TSetObject>> setExpr);
    IMultiUpdate<T, T1, T2, T3, T4, T5> Where(Expression<Func<T, T1, T2, T3, T4, T5, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, T5, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
public interface IMultiUpdate<T, T1, T2, T3, T4, T5, T6>
{
    IMultiUpdate<T, T1, T2, T3, T4, T5, T6> WithBy<TUpdateObject>(TUpdateObject updateObjs);
    IMultiUpdate<T, T1, T2, T3, T4, T5, T6> Set<TSetObject>(Expression<Func<T, T1, T2, T3, T4, T5, T6, TSetObject>> setExpr);
    IMultiUpdate<T, T1, T2, T3, T4, T5, T6> Where(Expression<Func<T, T1, T2, T3, T4, T5, T6, bool>> predicate);
    IMultiUpdate<T, T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T, T1, T2, T3, T4, T5, T6, bool>> predicate);
    IMultiQuery Execute();
    string ToSql();
}
