using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IUpdate<TEntity>
{
    IUpdateSet<TEntity> RawSql(string rawSql, object parameters);
    IUpdateSet<TEntity> WithBy<TUpdateObject>(TUpdateObject updateObjs, int bulkCount = 500);
    IUpdateSetting<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateSetting<TEntity> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T> From<T>();
    IUpdateFrom<TEntity, T1, T2> From<T1, T2>();
    IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>();
    IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();

    IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn);
    IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn);
}
public interface IUpdateSet<TEntity>
{
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateSetting<TEntity> : IUpdateSet<TEntity>
{
    IUpdateSetting<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateSetting<TEntity> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    IUpdateSetting<TEntity> Where(Expression<Func<IWhereSql, TEntity, bool>> predicate);
    IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate);
    IUpdateSetting<TEntity> And(bool condition, Expression<Func<IWhereSql, TEntity, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1>
{
    IUpdateFrom<TEntity, T1> Set<TSetObject>(Expression<Func<TEntity, T1, TSetObject>> setExpr);
    IUpdateFrom<TEntity, T1> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateFrom<TEntity, T1> Where(Expression<Func<IWhereSql, TEntity, T1, bool>> predicate);
    IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateJoin<TEntity, T1>
{
    IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdateJoin<TEntity, T1> Set<TSetObject>(Expression<Func<TEntity, T1, TSetObject>> setExpr);
    IUpdateJoin<TEntity, T1> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateJoin<TEntity, T1> Where(Expression<Func<IWhereSql, TEntity, T1, bool>> predicate);
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateFrom<TEntity, T1, T2>
{
    IUpdateFrom<TEntity, T1, T2> Set<TSetObject>(Expression<Func<TEntity, T1, T2, TSetObject>> setExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateJoin<TEntity, T1, T2>
{
    IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2> Set<TSetObject>(Expression<Func<TEntity, T1, T2, TSetObject>> setExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateFrom<TEntity, T1, T2, T3>
{
    IUpdateFrom<TEntity, T1, T2, T3> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, TSetObject>> setExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateJoin<TEntity, T1, T2, T3>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, TSetObject>> setExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, TSetObject>> setExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, TSetObject>> setExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TSetObject>> setExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TSetObject>> setExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TMember>(bool condition, Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<IWhereSql, TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}