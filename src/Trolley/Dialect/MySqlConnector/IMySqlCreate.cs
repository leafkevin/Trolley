using System;
using System.Collections;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreate<TEntity> : ICreate<TEntity>
{
    #region Sharding
    /// <summary>
    /// 手动指定分表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回插入对象</returns>
    new IMySqlCreate<TEntity> UseTable(string tableName);
    /// <summary>
    /// 手动指定分表规则参数值，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回插入对象</returns>
    new IMySqlCreate<TEntity> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表名获取委托，执行委托获取分表名，在批量插入场景，插入对象的值自动插入对应分表中，此方法只适用批量场景。
    /// 第一个参数是原始表名，第二个参数是插入的实体对象，返回值是分表名，如：.UseTable((tableName, insertObj) =&gt; $"{tableName}_{insertObj.CreatedAt:yyyyMM}")
    /// </summary>
    /// <typeparam name="TInsertObj">插入的实体类型</typeparam>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns>返回插入对象</returns>
    new IMySqlCreate<TEntity> UseTable<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则除WithBulk方法外的其他参数值，Trolley会自动结合otherFieldValues和WithBulk方法中的参数值确定分表名，otherFieldValues字段值数组中的顺序与配置的依赖字段(除WithBulk方法中依赖参数外)顺序一致，自插入到多个分表中，此方法只能用于批量场景。
    /// 如：假如分表规则根据租户ID+CreatedAt时间确定分表名，.UseTableByOthers([125])，125是租户ID，批量插入参数(WithBulk方法中的参数)中包含CreatedAt时间字段值
    /// </summary>
    /// <param name="otherFieldValues">分表依赖字段值获取委托</param>
    /// <returns>返回更新对象</returns>
    new IMySqlCreate<TEntity> UseTableByOthers(params object[] otherFieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回插入对象</returns>
    new IMySqlCreate<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region IgnoreInto
    /// <summary>
    /// 相同主键或唯一索引存在时不执行插入动作，INSERT IGNORE INTO...
    /// </summary>
    /// <returns>返回插入对象</returns>
    IMySqlCreate<TEntity> IgnoreInto();
    #endregion

    #region WithBy
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入
    /// <code>
    /// .WithBy(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     ...
    /// })
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`, ...) VALUES(@Name,@Age, ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    #endregion

    #region WithBulk
    /// <summary>
    /// 多条数据插入，自动增长栏位不需要传入，未列出属性不插入，分批次完成，每次插入bulkCount条数，适用于小批量数据插入
    /// <code>
    /// .WithBulk(new []{ new { ... }, new { ... }, new { ... })
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`, ...) VALUES (@ProductNo0,@Name0, ...),(@ProductNo1,@Name1, ...),(@ProductNo2,@Name2, ...)
    /// </code>
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500);
    #endregion

    #region WithBulkCopy
    /// <summary>
    /// 大批量数据插入，自动增长栏位不需要传入，未列出属性不插入，采用SqlBulkCopy方式，不生成SQL
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="timeoutSeconds">超时时间，单位秒</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null);
    #endregion

    #region From
    /// <summary>
    /// 从表T中查询数据创建子查询对象，如：
    /// <code>
    /// repository.From&lt;Menu&gt;() SQL:FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// </param>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T> From<T>();
    /// <summary>
    /// 使用2个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用3个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用4个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用5个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    /// <summary>
    /// 使用6个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    #endregion

    #region FromQuery
    /// <summary>
    /// 使用子查询subQuery作为创建子查询对象，子查询subQuery也可以是CTE表，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// repository.Create&lt;Menu&gt;().FromQuery(subQuery)
    /// SQL: INSERT INTO `sys_menu` SELECT ... FROM ( ... )
    /// </code>
    /// </summary>
    /// <typeparam name="T">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T> FromQuery<T>(IQuery<T> subQuery);
    /// <summary>
    /// 使用子查询多条数据插入，参数subQuery也可以是CTE表，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// repository.Create&lt;Menu&gt;(f =&gt; f.UseQuery(subQuery)).Select( ... )
    /// repository.Create&lt;Menu&gt;(f =&gt; f.From&lt;Page, Menu&gt;('o').Where(...)...)
    /// SQL: INSERT INTO `sys_menu` SELECT ... FROM ( ... )
    /// </code>
    /// </summary>
    /// <typeparam name="T">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    new IMySqlFromCommand<TEntity, T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    #endregion
}