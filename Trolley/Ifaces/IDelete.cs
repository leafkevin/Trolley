using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IDelete<T>
{
    IDelete<T> Where(Expression<Func<T, bool>> predicate);
    IDelete<T> Where(bool condition, Expression<Func<T, bool>> predicate);
    IDelete<T> Where(object keys);
    IDelete<T> Where(bool condition, object keys);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}

public interface IMultiDelete<T>
{
    IMultiDelete<T> Where(Expression<Func<T, bool>> predicate);
    IMultiDelete<T> Where(bool condition, Expression<Func<T, bool>> predicate);
    IMultiDelete<T> Where(object keys);
    IMultiDelete<T> Where(bool condition, object keys);
    //IMultiQuery Execute();
    string ToSql();
}