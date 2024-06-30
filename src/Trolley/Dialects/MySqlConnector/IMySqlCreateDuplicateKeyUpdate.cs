using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreateDuplicateKeyUpdate<TEntity>
{
    #region Values
    /// <summary>
    /// 使用VALUES筛选字段
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">要更新的字段</param>
    /// <returns>返回更新对象</returns>
    TField Values<TField>(TField fieldSelector);
    #endregion

    #region Alias
    /// <summary>
    /// 使用别名，固定别名为newRow，有些MySql，MariaDb版本不支持
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x => x.Alias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> UseAlias();
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，用法：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，用法：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    /// <summary>
    // VALUES多个字段更新，用法：
    /// <code>
    /// 不使用别名 .WithBy( ... ).OnDuplicateKeyUpdate(x =>x.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// 使用别名 .WithBy( ... ).OnDuplicateKeyUpdate(x => x.Alias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition的值，为true时设置VALUES多个字段更新
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// VALUES单个字段更新，可多次使用，用法：.WithBy( ... ).OnDuplicateKeyUpdate(x =>.Set(f => f.TotalAmount, f=> x.Values(f.TotalAmount)))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表表达式</param>
    /// <param name="fieldValueSelector">字段值表达式</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    /// <summary>
    /// VALUES单个字段更新，用法：
    /// </summary>
    /// <typeparam name="TFields">多个字段实体类型</typeparam>
    /// <param name="condition"></param>
    /// <param name="fieldsAssignment"></param>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector);
    #endregion
}