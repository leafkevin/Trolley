using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T">表T实体类型</typeparam>
/// <typeparam name="TMember">表T导航属性实体类型</typeparam>
public interface IIncludableQuery<T, TMember> : IQuery<T>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定TMember表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="TMember">表T2导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, TMember> : IQuery<T1, T2>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
    #endregion
}
/// <summary>
/// 导航属性查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="TMember">表T3导航属性实体类型</typeparam>
public interface IIncludableQuery<T1, T2, T3, TMember> : IQuery<T1, T2, T3>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, TMember> : IQuery<T1, T2, T3, T4>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, TMember> : IQuery<T1, T2, T3, T4, T5>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : IQuery<T1, T2, T3, T4, T5, T6>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定TMember表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定TMember表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 使用TMasterSharding主表分表名与当前表TMember分表映射关系表名获取委托获取当前表TMember分表表名，只需要指定主表分表的分表范围或条件即可，从表分表不需要指定分表名，会根据这个委托执行结果确定分表名称，通常适用于主表和从表都分表且一次查询多个分表的场景。第一个参数：dbKey，第二个参数是主表分表的原始名称，
    /// 如：分表sys_user_105，按租户分表，原始表sys_user，第三个参数是当前从表分表的原始名称，第四个参数是主表的分表名称，返回值是当前从表的分表名称，如：查询104，105租户的2020年1月以后的订单信息和买家信息，订单表按照租户+时间(年月)分表，用户按照租户分表
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .UseTable(f =&gt; (f.Contains("_104_") || f.Contains("_105_")) &amp;&amp; int.Parse(f.Substring(f.Length - 6)) &gt; 202001)
    ///     .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    ///     .UseTable&lt;Order&gt;((dbKey, orderOrigName, userOrigName, orderTableName) =&gt;
    ///     {
    ///         //sys_order_105_202001 -&gt; sys_user_105, sys_order_106_202002 -&gt; sys_user_106
    ///         var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///         return tableName.Substring(0, tableName.Length - 7);
    ///     })
    ///     ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">主表分表实体类型</typeparam>
    /// <param name="tableNameGetter">当前表分表名获取委托</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定TMember表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定TMember表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回导航属性查询对象</returns>
    new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region ThenInclude/ThenIncludeMany
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上，只支持1级。
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand) ...
    /// repository.From&lt;Brand&gt;()
    ///   .Include(f =&gt; f.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TNavigation">导航属性实体类型</typeparam>
    /// <param name="member">导航属性选择表达式，1:1,1:N关系都可以选择，如：f =&gt; f.Brand，f =&gt; f.Products</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member);
    /// <summary>
    /// 贪婪加载集合类导航属性，可继续贪婪加载元素类型中的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// <code>
    /// repository.From&lt;User&gt;()
    ///   .IncludeMany(f =&gt; f.Orders)
    ///   .Include(f =&gt; f.Product) //可继续加载订单中的产品信息
    ///   ...
    /// </code>
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="member">导航属性选择表达式，只能选择1:N关系，如：f =&gt; f.Products</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回导航属性查询对象</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null);
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
public interface IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
{
}