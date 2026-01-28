using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;
 
public interface IPostgreSqlCreateConflictDoUpdate<TEntity> : IIdentitiedCreated
{
    #region DoNothing
    IIdentitiedCreated DoNothing();
    #endregion

    #region UseKeys
    IPostgreSqlCreateConflictDoUpdate<TEntity> UseKeys();
    #endregion

    #region UseConstraint
    IPostgreSqlCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName);
    #endregion  

    #region Set
    /// <summary>
    ///多个字段更新，如：<code> .OnDuplicateKeyUpdate().Set(f => new { TotalAmount = f.TotalAmount + f.Excluded(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE "TotalAmount"=a."TotalAmount"+EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 多个字段更新，如：<code> .OnDuplicateKeyUpdate().Set(true, f => new { TotalAmount = f.TotalAmount + f.Excluded(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE "TotalAmount"=a."TotalAmount"+EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单个字段更新，可多次使用，如：
    /// <code>
    /// .OnConflictDoUpdate().Set(f => f.TotalAmount, f => f.Excluded(f.TotalAmount)))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON CONFLICT DO UPDATE "TotalAmount"=EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表表达式</param>
    /// <param name="fieldValueSelector">字段值表达式</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    #endregion

    #region Where
    IPostgreSqlCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    #endregion

    #region Returning
    IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}
public interface IPostgreSqlBulkCreateConflictDoUpdate<TEntity> : ICreated
{
    #region DoNothing
    ICreated DoNothing();
    #endregion

    #region UseKeys
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseKeys();
    #endregion

    #region UseConstraint
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName);
    #endregion  

    #region Set
    /// <summary>
    /// 多个字段更新，可多次使用，如：
    /// <code>
    /// 使用Excluded方法 .WithBy( ... ).OnDuplicateKeyUpdate(x =>x.Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE "TotalAmount"=a."TotalAmount"+EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition的值，为true时多个字段更新，可多次使用，如：
    /// <code>
    /// 使用Excluded方法 .WithBy( ... ).OnDuplicateKeyUpdate(x =>x.Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE "TotalAmount"=a."TotalAmount"+EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单个字段更新，可多次使用，如：
    /// <code>
    /// .WithBy( ... ).OnConflictDoUpdate(x =>.Set(f => f.TotalAmount, f => x.Excluded(f.TotalAmount)))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON CONFLICT DO UPDATE "TotalAmount"=EXCLUDED."TotalAmount"
    /// </code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表表达式</param>
    /// <param name="fieldValueSelector">字段值表达式</param>
    /// <returns>返回更新对象</returns>
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    #endregion

    #region Where
    IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    #endregion

    #region Returning
    IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}