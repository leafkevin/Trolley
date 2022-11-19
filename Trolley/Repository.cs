using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region 字段
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private IDbTransaction transaction;
    #endregion

    #region 属性
    public IOrmProvider OrmProvider => this.connection.OrmProvider;
    public IDbConnection Connection => this.connection;
    public IDbTransaction Transaction => this.transaction;
    #endregion

    #region 构造方法
    internal Repository(IOrmDbFactory dbFactory, TheaConnection connection)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
    }
    #endregion

    #region Query
    public IQuery<T> From<T>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T));
        return new Query<T>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2));
        return new Query<T1, T2>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3));
        return new Query<T1, T2, T3>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new Query<T1, T2, T3, T4>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new Query<T1, T2, T3, T4, T5>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return new Query<T1, T2, T3, T4, T5, T6>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return new Query<T1, T2, T3, T4, T5, T6, T7>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.dbFactory, this.connection, this.transaction, visitor);
    }
    //public IQueryReader QueryMultiple(Action<IMultiQuery> queries)
    //{
    //    if (queries == null) throw new ArgumentNullException(nameof(queries));
    //    var multiQuery = new MultiQuery();
    //    queries.Invoke(multiQuery);
    //    //TODO:
    //    multiQuery.ToSql();
    //    var visitor = new SqlExpressionVisitor(this.dbFactory, this.connection.OrmProvider);
    //    return new SqlExpression<TEntity>(this.dbFactory, this.connection, visitor.From(typeof(TEntity), sqlType));
    //}
    //public Task<IQueryReader> QueryMultipleAsync(Action<IMultiQuery> queries, CancellationToken cancellationToken = default)
    //{
    //    throw new NotImplementedException();
    //}
    #endregion

    #region Get
    public TEntity Get<TEntity>(object whereObj)
    {
        var visitor = new QueryVisitor(this.dbFactory, this.OrmProvider);
        visitor.From(typeof(TEntity));
        //visitor.Where()
        Expression<Func<TEntity, TEntity>> defaultExpr = f => f;
        visitor.Select(null, defaultExpr);
        visitor.BuildSql(out var dbParameters, out var readFields);
        throw new NotImplementedException();
    }

    public Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Create
    public int Create<TEntity>(object entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateAsync<TEntity>(object entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ICreate<T> Create<T>()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Update
    public int UpdateByKey<TEntity>(object entities)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateByKeyAsync<TEntity>(object entities, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Update<TEntity, TFields>(object updateObj, object whereObj)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync<TEntity, TFields>(object updateObj, object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Update<TEntity>(object updateOnly, Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync<TEntity>(object updateOnly, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Update<TEntity, TFields>(object updateObj, string[] updateFields, Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync<TEntity, TFields>(object updateObj, string[] updateFields, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Update<TEntity, TFields>(object updateObj, Expression<Func<TEntity, TFields>> updateFields, Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync<TEntity, TFields>(object updateObj, Expression<Func<TEntity, TFields>> updateFields, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IUpdate<T> Update<T>()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Delete
    public int DeleteByKey<TEntity>(object keys)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteByKeyAsync<TEntity>(object keys, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Delete<TEntity>(object whereObj)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int Delete<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IDelete<T> Delete<T>()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Exists
    public bool Exists<TEntity>(object anonymousObj)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Execute
    public int Execute(string sql, object parameters = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> ExecuteAsync(string sql, object parameters = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Others
    public void Close()
    {
        throw new NotImplementedException();
    }

    public Task CloseAsync()
    {
        throw new NotImplementedException();
    }

    public void Timeout(int timeout)
    {
        throw new NotImplementedException();
    }

    public void Begin()
    {
        throw new NotImplementedException();
    }

    public void Commit()
    {
        throw new NotImplementedException();
    }

    public void Rollback()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
    #endregion
}
