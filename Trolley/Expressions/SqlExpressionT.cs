using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class SqlExpression<T> : ISqlExpression<T>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly SqlExpressionVisitor visitor;
    public SqlExpression(IOrmDbFactory dbFactory, TheaConnection connection, SqlExpressionVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.visitor = visitor;
    }

    public bool Exists(Expression<Func<T, bool>> predicate)
    {
        throw new NotImplementedException();
    }
    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// 导航属性的InnerJoin
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public ISqlExpression<T> InnerJoin(Expression<Func<T, bool>> predicate)
    {
        this.visitor.Join(typeof(T), predicate.Body, "INNER JOIN");
        return this;
    }
    public ISqlExpression<T> LeftJoin(Expression<Func<T, bool>> predicate)
    {
        this.visitor.Join(typeof(T), predicate.Body, "LEFT JOIN");
        return this;
    }
    public ISqlExpression<T> RightJoin(Expression<Func<T, bool>> predicate)
    {
        this.visitor.Join(typeof(T), predicate, "RIGHT JOIN");
        return this;
    }
    public ISqlExpression<T> Include<TTarget>(Expression<Func<T, TTarget>> memberSelector)
    {
        this.visitor.Include(memberSelector);
        return this;
    }
    public ISqlExpression<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        this.visitor.Select(fieldsExpr.Body);
        return new SqlExpression<TTarget>(this.dbFactory, this.connection, this.visitor);
    }
    public IGroupBySqlExpression<TTarget> GroupBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        throw new NotImplementedException();
    }
    public ISqlExpression<T> OrderBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        throw new NotImplementedException();
    }
    public ISqlExpression<T> OrderByDescending<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
    {
        throw new NotImplementedException();
    }
    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector)
    {
        throw new NotImplementedException();
    }
    public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public long Count()
    {
        var command = this.connection.CreateCommand();

        throw new NotImplementedException();
    }
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public T First()
    {
        throw new NotImplementedException();
    }
    public Task<T> FirstAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public List<T> ToList()
    {
        Expression<Func<T, T>> defaultExpr = f => f;
        var sql = this.visitor.BuildSql(defaultExpr);
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
        var result = new List<T>();
        this.connection.Open();
        var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(reader.To<T>(this.connection, this.visitor.ReaderFields));
        }
        reader.Close();
        return result;
    }
    public Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public ISqlExpression<T> Where(Expression<Func<T, bool>> predicate)
        => this.Where(true, predicate);
    public ISqlExpression<T> Where(bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition) this.visitor.Where(predicate);
        return this;
    }
    public string ToSql() => this.visitor.BuildSql();
}


public class SqlExpression<T1, T2>
{

}
