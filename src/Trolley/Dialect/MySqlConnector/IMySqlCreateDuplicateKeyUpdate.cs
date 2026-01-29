using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreateDuplicateKeyUpdate : IIdentitiedCreated
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .OnDuplicateKeyUpdate().UseAlias().Set(f =&gt; new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <param name="aliasName">别名</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate Set(bool condition, object updateObj);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate Set<TField>(string fieldName, object fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate Set<TField>(bool condition, string fieldName, object fieldValue);
    #endregion

    #region Set
    /// <summary>
    /// 返回更新后的数据
    /// </summary>
    /// <typeparam name="TResult">数据类型</typeparam>
    /// <param name="fieldNames">返回的字段名称列表</param>
    /// <returns>返回更新对象</returns>
    IResultCommand<TResult> Returning<TResult>(string fieldNames);
    #endregion
}
public interface IMySqlBulkCreateDuplicateKeyUpdate : ICreated
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .OnDuplicateKeyUpdate().UseAlias().Set(f =&gt; new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <param name="aliasName">别名</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate Set(bool condition, object updateObj);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate Set<TField>(string fieldName, object fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate Set<TField>(bool condition, string fieldName, object fieldValue);
    #endregion

    #region Set
    /// <summary>
    /// 返回更新后的数据
    /// </summary>
    /// <typeparam name="TResult">数据类型</typeparam>
    /// <param name="fieldNames">返回的字段名称列表</param>
    /// <returns>返回更新对象</returns>
    IBulkResultCommand<TResult> Returning<TResult>(string fieldNames);
    #endregion
}

public interface IMySqlCreateDuplicateKeyUpdate<TEntity> : IMySqlCreateDuplicateKeyUpdate
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .OnDuplicateKeyUpdate().UseAlias().Set(f =&gt; new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <param name="aliasName">别名</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(string fieldName, object fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, string fieldName, object fieldValue);

    /// <summary>
    // VALUES多个字段更新，如：
    /// <code>
    /// 不使用别名 .OnDuplicateKeyUpdate().Set(f =&gt; new { TotalAmount = f.Values(f.TotalAmount) }))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// 使用别名 .OnDuplicateKeyUpdate().UseAlias("newRow").Set(f =&gt; new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
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
    /// 单个字段更新，如：.Set(f =&gt;> f.OrderName, "12345"))
    /// </summary>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue);
    /// <summary>
    /// 单个字段更新，如：.Set(true, f =&gt;> f.OrderName, "12345"))
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue);
    /// <summary>
    /// 单个字段更新，如：.Set(f =&gt;> f.Products, f =&gt; f.Values(f.Products)))
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="valueGetter">字段值获取表达式</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter);
    /// <summary>
    /// 单个字段更新，如：.Set(true, f =&gt;> f.Products, f =&gt; f.Values(f.Products)))
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="valueGetter">字段值获取表达式</param>
    /// <returns>返回更新对象</returns>
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter);
    #endregion

    #region Set
    IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}
public interface IMySqlBulkCreateDuplicateKeyUpdate<TEntity> : IMySqlBulkCreateDuplicateKeyUpdate
{
    #region UseAlias
    /// <summary>
    /// 使用别名，有些MySql，MariaDb版本不支持
    /// <code>
    /// .OnDuplicateKeyUpdate().UseAlias("newRow").Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
    /// SQL: INSERT INTO ... VALUES ( ... ) AS newRow ON DUPLICATE KEY UPDATE `TotalAmount`=`TotalAmount`+newRow.TotalAmount
    /// </code>
    /// </summary>
    /// <returns>返回更新对象</returns>
    new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> UseAlias(string aliasName = "newRow");
    #endregion

    #region Set
    /// <summary>
    /// 使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，如：
    /// <code>.Set(new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id); .Set(new User { Id = 2, Name = "kevin", SourceType = null }).Where(f =&gt; f.Id);  
    /// SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId </code>
    /// </summary>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(object updateObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用更新对象updateObj部分字段更新，updateObj对象中除OnlyFields、IgnoreFields、Where方法筛选外的所有字段都将参与更新，单对象更新，需要配合where条件使用，为false不做更新，如：
    /// <code>.Set(true, new { Id = 1, Name = "kevin", SourceType = DBNull.Value }).Where(f =&gt; f.Id);  SQL: SET `Name`=@Name,`SourceType`=@SourceType WHERE `Id`=@kId
    /// .Set(true, new User { Id = 1, ... })  SQL: SET ... //只更新部分字段，可以使用OnlyFields方法，忽略部分字段，可以使用IgnoreFields方法</code>
    /// </summary>
    /// <typeparam name="TUpdateObj">更新对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="updateObj">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新，可以是字典或是匿名对象或是现有命名对象</param>
    /// <returns>返回更新对象</returns>
    new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set(bool condition, object updateObj);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(string fieldName, object fieldValue);
    /// <summary>
    /// 单个字段更新
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldName">字段名</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    new IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, string fieldName, object fieldValue);

    /// <summary>
    // VALUES多个字段更新，如：
    /// <code>
    /// 不使用别名 .OnDuplicateKeyUpdate().Set(f => new { TotalAmount = f.Values(f.TotalAmount) }))
    /// SQL: INSERT INTO ... VALUES ( ... ) ON DUPLICATE KEY UPDATE `TotalAmount`=VALUES(`TotalAmount`)
    /// 使用别名 .OnDuplicateKeyUpdate().UseAlias("newRow").Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
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
    /// 单个字段更新，如：.Set(f =&gt;> f.OrderName, "12345"))
    /// </summary>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, object fieldValue);
    /// <summary>
    /// 单个字段更新，如：.Set(true, f =&gt;> f.OrderName, "12345"))
    /// </summary>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, object fieldValue);
    /// <summary>
    /// 单个字段更新，如：.Set(f =&gt;> f.Products, f =&gt; f.Values(f.Products)))
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="valueGetter">字段值获取表达式</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter);
    /// <summary>
    /// 单个字段更新，如：.Set(true, f =&gt;> f.Products, f =&gt; f.Values(f.Products)))
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式</param>
    /// <param name="valueGetter">字段值获取表达式</param>
    /// <returns>返回更新对象</returns>
    IMySqlBulkCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, object>> valueGetter);
    #endregion

    #region Set
    IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}