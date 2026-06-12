using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public interface IIncludableQueryBase
{
    #region Properties
    /// <summary>
    /// 是否一对多关系导航
    /// </summary>
    bool IsIncludeMany { get; }
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T">表T实体类型</typeparam>
/// <typeparam name="TMember">表T导航属性实体类型</typeparam>
public interface IIncludableQuery<T, TMember> : IIncludableQueryBase, IQuery<T>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取分表名。委托第一个参数是首个分表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;()
    /// .UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .Include(f =&gt; f.Buyer)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">当前<typeparamref name="TMember"/>表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="TMember">表T2导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, TMember> : IIncludableQueryBase, IQuery<T1, T2>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="TMember">表T3导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="TMember">表T4导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="TMember">表T5导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="TMember">表T6导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="TMember">表T7导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="TMember">表T8导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="TMember">表T9导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="TMember">表T10导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="TMember">表T11导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="TMember">表T12导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="TMember">表T13导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="TMember">表T14导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
/// <typeparam name="TMember">表T15导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Sharding
    /// <summary>
    /// 直接指定<typeparamref name="TMember"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个多分表与当前<typeparamref name="TMember"/>表名的映射关系，指定当前<typeparamref name="TMember"/>表分表名获取委托，执行委托获取当前<typeparamref name="TMember"/>表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前<typeparamref name="TMember"/>表原始表名，第三个参数是首个多分表当前分表名，返回值是当前<typeparamref name="TMember"/>表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// })
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a LEFT JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a LEFT JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <param name="tableNameGetter">TMember表分表名获取委托</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="TMember"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="TMember"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(params object[] fieldValues);
    /// <summary>
    /// 指定使用UNION连接分表查询语句，默认使用UNION ALL连接分表查询语句
    /// </summary>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseUnionShardingTable();
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema);
    #endregion

    #region WithTableAliasTrailing
    /// <summary>
    /// 在表别名后面追加原始SQL片段，rawSql不能为null或空字符串，如：SQL SERVER数据库，.WithTableAliasTrailing("WITH(NOLOCK)")，生成SQL为：FROM `sys_order` a WITH(NOLOCK)
    /// </summary>
    /// <param name="rawSql">原始SQL片段</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> WithTableAliasTrailing(string rawSql);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 继续加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .Include(f =&gt; f.Brand) ...
    /// .Include(f =&gt; f.Products) ...
    /// .Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> memberSelector);
    /// <summary>
    /// 继续加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .ThenIncludeMany(f =&gt; f.Orders)  //与 .Include(f =&gt; f.Orders) 等价
    /// .ThenIncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，加载满足条件的子表数据</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
/// <typeparam name="T16">表T16实体类型</typeparam>
/// <typeparam name="TMember">表T16导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : IIncludableQueryBase, IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
}