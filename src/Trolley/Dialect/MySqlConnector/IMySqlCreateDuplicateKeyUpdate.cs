using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreateDuplicateKeyUpdate<TEntity> : IContinuedCreate<TEntity>
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x => x.UseAlias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    // VALUES多个字段更新，如：
    /// <code>
    /// 不使用别名 .WithBy( ... ).OnDuplicateKeyUpdate(x =>x.Set(f => new { TotalAmount = x.Values(f.TotalAmount) }))
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
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="fieldSelector"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="condition"></param>
    /// <param name="fieldSelector"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion
}
public interface IMySqlBulkCreateDuplicateKeyUpdate<TEntity> : IBulkContinuedCreate<TEntity>
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .WithBy( ... ).OnDuplicateKeyUpdate(x => x.UseAlias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    // VALUES多个字段更新，如：
    /// <code>
    /// 不使用别名 .WithBy( ... ).OnDuplicateKeyUpdate(x =>x.Set(f => new { TotalAmount = x.Values(f.TotalAmount) }))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// 使用别名 .WithBy( ... ).OnDuplicateKeyUpdate(x => x.Alias().Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 判断condition的值，为true时设置VALUES多个字段更新
    /// </summary>
    /// <typeparam name="TFields">要更新的实体类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldsAssignment">要更新的字段表达式，尽力使用VALUES</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="fieldSelector"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <param name="condition"></param>
    /// <param name="fieldSelector"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion
}