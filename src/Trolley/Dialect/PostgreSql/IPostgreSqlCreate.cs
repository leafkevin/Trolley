using System;
using System.Collections;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public interface IPostgreSqlCreate<TEntity> : ICreate<TEntity>
{
    #region Sharding
    /// <summary>
    /// 手动指定<typeparamref name="TEntity"/>表分表名，如：.UseTable("sys_order_202001")
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlCreate<TEntity> UseTable(string tableName);
    /// <summary>
    /// 手动指定<typeparamref name="TEntity"/>表分表名获取委托，使用委托获取分表名，在批量插入场景，插入对象的值自动插入对应分表中，此方法只适用批量场景
    /// </summary>
    /// <typeparam name="TInsertObj">插入的实体类型</typeparam>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <exception cref="ArgumentNullException"></exception>
    new IPostgreSqlCreate<TEntity> UseTable<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TEntity"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlCreate<TEntity> UseTableBy(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    new IPostgreSqlCreate<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region WithBy
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，命名或匿名对象都可以
    /// <para>自动增长的栏位，不需要传入，如：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         ...
    ///     })
    ///     .Execute();
    /// SQL: INSERT INTO "sys_user" ("Name","Age", ...) VALUES(@Name,@Age, ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值，命名或匿名对象都可以</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
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
    new IPostgreSqlBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500);
    #endregion

    #region WithBulkCopy
    /// <summary>
    /// 批量插入，采用SqlBulkCopy方式，不生成SQL
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> WithBulkCopy(IEnumerable insertObjs);
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
    new IPostgreSqlFromCommand<T> From<T>();
    /// <summary>
    /// 使用2个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlFromCommand<T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用3个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlFromCommand<T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用4个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用5个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
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
    new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    #endregion

    #region FromQuery
    /// <summary>
    /// 使用子查询subQuery作为创建子查询对象，子查询subQuery也可以是CTE表，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// repository.Create&lt;Menu&gt;().FromQuery(subQuery)
    /// SQL: INSERT INTO "sys_menu" SELECT ... FROM ( ... )
    /// </code>
    /// </summary>
    /// <typeparam name="T">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlFromCommand<T> FromQuery<T>(IQuery<T> subQuery);
    /// <summary>
    /// 使用子查询subQuery作为创建子查询对象，子查询subQuery也可以是CTE表，如：
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
    new IPostgreSqlFromCommand<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    #endregion
}
