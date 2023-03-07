using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IUpdate<TEntity>
{
    IUpdateSet<TEntity> WithBy<TFields>(TFields parameters, int bulkCount = 500);
    IUpdateSet<TEntity> WithBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, object parameters, int bulkCount = 500);

    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

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
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IUpdateSetting<TEntity> : IUpdateSet<TEntity>
{
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateFrom<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate);
}
public interface IUpdateJoin<TEntity, T1> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);

    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1, T2> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate);
}
public interface IUpdateJoin<TEntity, T1, T2> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1, T2, T3> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
}
public interface IUpdateJoin<TEntity, T1, T2, T3> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4, T5> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> subQueryExpr);

    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
}