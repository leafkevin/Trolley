using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreateDuplicateKeyUpdate<TEntity>
{
    #region Values
    TField Values<TField>(TField fieldSelector);
    #endregion

    #region Alias
    /// <summary>
    /// 使用别名，有些MySql,MariaDb版本不支持
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x => x.Alias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Alias();
    #endregion

    #region Set
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    // 不带别名
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x =>.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// </code>
    /// 带有别名
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x => x.Alias("row").Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS row ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+row.TotalAmount
    /// </code>
    /// </summary>
    /// <typeparam name="TFields"></typeparam>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x =>.Set(f => f.TotalAmount, f=> x.Values(f.TotalAmount)))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="fieldSelector"></param>
    /// <param name="fieldValueSelector"></param>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    #endregion
}