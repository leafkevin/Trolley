using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IDelete<TEntity>
{
    IDeleted<TEntity> RawSql(string rawSql, object parameters);
    IDeleted<TEntity> Where(object keys);
    IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
public interface IDeleted<TEntity>
{
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
public interface IDeleting<TEntity>
{
    IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}
//public interface IMultiDelete<T>
//{
//    IMultiDelete<T> Where(Expression<Func<T, bool>> predicate);
//    IMultiDelete<T> Where(bool condition, Expression<Func<T, bool>> predicate);
//    IMultiDelete<T> Where(object keys);
//    IMultiDelete<T> Where(bool condition, object keys);
//    //IMultiQuery Execute();
//    string ToSql();
//}