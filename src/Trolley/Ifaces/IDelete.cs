using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IDelete<TEntity>
{
    IDeleted<TEntity> Where(object keys);
    IDeleting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
}
public interface IDeleted<TEntity>
{
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IDeleting<TEntity>
{
    IDeleting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}