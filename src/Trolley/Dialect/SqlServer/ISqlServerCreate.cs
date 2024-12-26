using System.Collections;

namespace Trolley.SqlServer;

public interface ISqlServerCreate<TEntity> : ICreate<TEntity>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TEntity表分表名执行插入操作，完整的表名，如：.UseTable("sys_order_202001")，按月分表
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerCreate<TEntity> UseTable(string tableName);
    /// <summary>
    /// 根据字段值确定TEntity表分表名执行插入操作，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerCreate<TEntity> UseTableBy(object field1Value, object field2Value = null);
    #endregion

    #region WithLock
    ISqlServerCreate<TEntity> WithLock(string lockName);
    #endregion

    #region WithBy
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，命名或匿名对象都可以
    /// <para>自动增长的栏位，不需要传入，用法：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         ...
    ///     })
    ///     .Execute();
    /// SQL: INSERT INTO [sys_user] ([Name],[Age], ...) VALUES(@Name,@Age, ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值，命名或匿名对象都可以</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    #endregion

    #region WithBulk
    /// <summary>
    /// 批量插入，采用多表值方式，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name], ...) VALUES (@ProductNo0,@Name0, ...),(@ProductNo1,@Name1, ...),(@ProductNo2,@Name2, ...)
    /// </code>
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500);
    #endregion

    #region WithBulkCopy
    /// <summary>
    /// 批量插入，采用SqlBulkCopy方式，不生成SQL
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="timeoutSeconds">超时时间，单位秒</param>
    /// <returns>返回插入对象</returns>
    ISqlServerCreated<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null);
    #endregion

    #region From
    /// <summary>
    /// 从表T中查询数据创建子查询对象，用法：
    /// <code>
    /// repository.From&lt;Menu&gt;() SQL:FROM [sys_menu]
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// </param>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T> From<T>();
    /// <summary>
    /// 使用2个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用3个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用4个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用5个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
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
    new ISqlServerFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    /// <summary>
    /// 使用子查询subQuery作为创建子查询对象，子查询subQuery也可以是CTE表，用法：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// repository.Create&lt;Function&gt;(subQuery).Select( ... )
    /// SQL: INSERT INTO [sys_menu] SELECT ... FROM ( ... )
    /// </code>
    /// </summary>
    /// <typeparam name="T">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    new ISqlServerFromCommand<T> From<T>(IQuery<T> subQuery);
    #endregion
}
