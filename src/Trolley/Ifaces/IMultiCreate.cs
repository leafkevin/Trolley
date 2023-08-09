using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface IMultiCreate<TEntity>
{
    /// <summary>
    /// 使用原始SQL插入数据，用法：
    /// <code>
    /// repository.Insert&lt;Order&gt;()
    ///     .RawSql("INSERT INTO Table(Field1,Field2) VALUES(1,'xxx')");
    /// </code>
    /// </summary>
    /// <param name="rawSql">原始SQL</param>
    /// </param>
    /// <returns>返回插入对象</returns>
    IMultiCreated<TEntity> RawSql(string rawSql);
    /// <summary>
    /// 使用原始SQL和参数插入数据，用法：
    /// <code>
    /// repository.Insert&lt;Order&gt;()
    ///     .RawSql("INSERT INTO Table(Field1,Field2) VALUES(@Value1,@Value2)", new { Value1 = 1, Value2 = "xxx" });
    /// </code>
    /// </summary>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">SQL中使用的参数，匿名对象或是实体对象，不支持某个变量值，如：
    /// <code>new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</code>
    /// </param>
    /// <returns>返回插入对象</returns>
    IMultiCreated<TEntity> RawSql(string rawSql, object parameters);
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入
    /// <para>自动增长的栏位，不需要传入，用法：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         CompanyId = 1,
    ///         Gender = Gender.Male,
    ///         IsEnabled = true,
    ///         CreatedAt = DateTime.Now,
    ///         CreatedBy = 1,
    ///         UpdatedAt = DateTime.Now,
    ///         UpdatedBy = 1
    ///     })
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_user` (`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 批量插入,一次性插入所有数据，不做分批处理，采用多表值方式，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
    /// </code>
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <returns></returns>
    IMultiCreated<TEntity> WithBulkBy(IEnumerable insertObjs);
    /// <summary>
    /// 从<paramref>TSource</paramref>表查询数据，并插入当前表中,用法：
    /// <code>
    /// repository.Create&lt;Product&gt;()
    ///     .From&lt;Brand&gt;(f =&gt; new
    ///     {
    ///         ProductNo = "PN_" + f.BrandNo,
    ///         Name = "PName_" + f.Name,
    ///         BrandId = f.Id,
    ///         CategoryId = 1,
    ///         f.CompanyId,
    ///         f.IsEnabled,
    ///         f.CreatedBy,
    ///         f.CreatedAt,
    ///         f.UpdatedBy,
    ///         f.UpdatedAt
    ///     })
    ///     .Where(f =&gt; f.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TSource">实体类型，数据来源表</typeparam>
    /// <param name="fieldSelector">插入的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector);
    /// <summary>
    /// 从表T1, T2表查询数据，并插入当前TEntity表中,用法：
    /// <code>
    /// repository.Create&lt;Product&gt;()
    ///     .From&lt;Brand&gt;(f =&gt; new
    ///     {
    ///         ProductNo = "PN_" + f.BrandNo,
    ///         Name = "PName_" + f.Name,
    ///         BrandId = f.Id,
    ///         CategoryId = 1,
    ///         f.CompanyId,
    ///         f.IsEnabled,
    ///         f.CreatedBy,
    ///         f.CreatedAt,
    ///         f.UpdatedBy,
    ///         f.UpdatedAt
    ///     })
    ///     .Where(f =&gt; f.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="fieldSelector">插入的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector);
    /// <summary>
    /// 从表T1, T2, T3表查询数据，并插入当前TEntity表中,用法：
    /// <code>
    /// repository.Create&lt;Product&gt;()
    ///     .From&lt;Brand&gt;(f =&gt; new
    ///     {
    ///         ProductNo = "PN_" + f.BrandNo,
    ///         Name = "PName_" + f.Name,
    ///         BrandId = f.Id,
    ///         CategoryId = 1,
    ///         f.CompanyId,
    ///         f.IsEnabled,
    ///         f.CreatedBy,
    ///         f.CreatedAt,
    ///         f.UpdatedBy,
    ///         f.UpdatedAt
    ///     })
    ///     .Where(f =&gt; f.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="fieldSelector">插入的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector);
    /// <summary>
    /// 从表T1, T2, T3, T4表查询数据，并插入当前TEntity表中,用法：
    /// <code>
    /// repository.Create&lt;Product&gt;()
    ///     .From&lt;Brand&gt;(f =&gt; new
    ///     {
    ///         ProductNo = "PN_" + f.BrandNo,
    ///         Name = "PName_" + f.Name,
    ///         BrandId = f.Id,
    ///         CategoryId = 1,
    ///         f.CompanyId,
    ///         f.IsEnabled,
    ///         f.CreatedBy,
    ///         f.CreatedAt,
    ///         f.UpdatedBy,
    ///         f.UpdatedAt
    ///     })
    ///     .Where(f =&gt; f.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="fieldSelector">插入的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector);
    /// <summary>
    /// 从表T1, T2, T3, T4, T5表查询数据，并插入当前TEntity表中,用法：
    /// <code>
    /// repository.Create&lt;Product&gt;()
    ///     .From&lt;Brand&gt;(f =&gt; new
    ///     {
    ///         ProductNo = "PN_" + f.BrandNo,
    ///         Name = "PName_" + f.Name,
    ///         BrandId = f.Id,
    ///         CategoryId = 1,
    ///         f.CompanyId,
    ///         f.IsEnabled,
    ///         f.CreatedBy,
    ///         f.CreatedAt,
    ///         f.UpdatedBy,
    ///         f.UpdatedAt
    ///     })
    ///     .Where(f =&gt; f.Id == 1)
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_product` (`ProductNo`,`Name`,`BrandId`,`CategoryId`,`CompanyId`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt`) SELECT CONCAT('PN_',a.`BrandNo`),CONCAT('PName_',a.`Name`),a.`Id`,@CategoryId,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt` FROM `sys_brand` a WHERE a.`Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <param name="fieldSelector">插入的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity>
{
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入
    /// <para>自动增长的栏位，不需要传入，用法：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         CompanyId = 1,
    ///         Gender = Gender.Male,
    ///         IsEnabled = true,
    ///         CreatedAt = DateTime.Now,
    ///         CreatedBy = 1,
    ///         UpdatedAt = DateTime.Now,
    ///         UpdatedBy = 1
    ///     })
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_user` (`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用插入对象部分字段插入，单个对象插入
    /// <para>自动增长的栏位，不需要传入，用法：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(true, new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         CompanyId = 1,
    ///         Gender = Gender.Male,
    ///         IsEnabled = true,
    ///         CreatedAt = DateTime.Now,
    ///         CreatedBy = 1,
    ///         UpdatedAt = DateTime.Now,
    ///         UpdatedBy = 1
    ///     })
    ///     .Execute();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_user` (`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface IMultiCreated<TEntity>
{
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入数据表TEntity实体类型</typeparam>
/// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity, TSource>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, TSource> Where(bool condition, Expression<Func<TSource, bool>> ifPredicate, Expression<Func<TSource, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, TSource> And(Expression<Func<TSource, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, TSource> And(bool condition, Expression<Func<TSource, bool>> ifPredicate = null, Expression<Func<TSource, bool>> elsePredicate = null);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入数据的实体类型</typeparam>
/// <typeparam name="T1">数据来源表T1实体类型</typeparam>
/// <typeparam name="T2">数据来源表T2实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity, T1, T2>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入数据的实体类型</typeparam>
/// <typeparam name="T1">数据来源表T1实体类型</typeparam>
/// <typeparam name="T2">数据来源表T2实体类型</typeparam>
/// <typeparam name="T3">数据来源表T3实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity, T1, T2, T3>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入数据的实体类型</typeparam>
/// <typeparam name="T1">数据来源表T1实体类型</typeparam>
/// <typeparam name="T2">数据来源表T2实体类型</typeparam>
/// <typeparam name="T3">数据来源表T3实体类型</typeparam>
/// <typeparam name="T4">数据来源表T4实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity, T1, T2, T3, T4>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入数据的实体类型</typeparam>
/// <typeparam name="T1">数据来源表T1实体类型</typeparam>
/// <typeparam name="T2">数据来源表T2实体类型</typeparam>
/// <typeparam name="T3">数据来源表T3实体类型</typeparam>
/// <typeparam name="T4">数据来源表T4实体类型</typeparam>
/// <typeparam name="T5">数据来源表T5实体类型</typeparam>
public interface IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5>
{
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回插入对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiContinuedCreate<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute();
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}