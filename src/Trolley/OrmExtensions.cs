using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class OrmExtensions
{
    public static TEntity QueryFirst<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate = null)
        => repository.From<TEntity>().Where(predicate).First();
    public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        => await repository.From<TEntity>().Where(predicate).FirstAsync(cancellationToken);
    public static List<TEntity> Query<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate = null)
        => repository.From<TEntity>().Where(predicate).ToList();
    public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        => await repository.From<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    public static Dictionary<TKey, TValue> QueryDictionary<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector) where TKey : notnull
        => repository.From<TEntity>().Where(predicate).ToDictionary(keySelector, valueSelector);
    public static async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
        => await repository.From<TEntity>().Where(predicate).ToDictionaryAsync(keySelector, valueSelector, cancellationToken);

    public static int Create<TEntity>(this IRepository repository, object parameter)
        => repository.Create<TEntity>().WithBy(parameter).Execute();
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, object parameter, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithBy(parameter).ExecuteAsync(cancellationToken);
    public static int Create<TEntity>(this IRepository repository, string rawSql, object parameter)
        => repository.Create<TEntity>().RawSql(rawSql, parameter).Execute();
    /// <summary>
    /// 使用原始SQL插入数据，如：repository.Insert&lt;Order&gt;().RawSql("INSERT INTO Table(Field1,Field2) VALUES(@Value1,@Value2)", new { Value1 = 1, Value2 = "xxx" });
    /// </summary>
    /// <typeparam name="TEntity">实体类型，需要有对应的模型映射找到要插入的表</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameter">SQL中使用的参数，匿名对象或是实体对象，不支持某个变量值
    /// 如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, string rawSql, object parameter, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().RawSql(rawSql, parameter).ExecuteAsync(cancellationToken);
    public static int Create<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500)
        => repository.Create<TEntity>().WithByBulk(entities, bulkCount).Execute();
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithByBulk(entities, bulkCount).ExecuteAsync(cancellationToken);

    public static int Update<TEntity>(this IRepository repository, object entity)
        => repository.Update<TEntity>().WithBy(entity).Execute();
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, object entity, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(entity).ExecuteAsync(cancellationToken);
    public static int Update<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsExpr, Expression<Func<TEntity, bool>> predicate)
        => repository.Update<TEntity>().Set(fieldsExpr).Where(predicate).Execute();
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsExpr, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().Set(fieldsExpr).Where(predicate).ExecuteAsync(cancellationToken);
    public static int Update<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsExpr, object parameters, int bulkCount = 500)
        => repository.Update<TEntity>().WithBy(fieldsExpr, parameters, bulkCount).Execute();
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsExpr, object parameters, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(fieldsExpr, parameters, bulkCount).ExecuteAsync(cancellationToken);

    public static int Delete<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate)
        => repository.Delete<TEntity>().Where(predicate).Execute();
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(predicate).ExecuteAsync(cancellationToken);
    public static int Delete<TEntity>(this IRepository repository, object keys)
        => repository.Delete<TEntity>().Where(keys).Execute();
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, object keys, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(keys).ExecuteAsync(cancellationToken);
}
