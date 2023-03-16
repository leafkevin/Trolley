using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">实体类型，需要有模型映射</typeparam>
public interface ICreate<TEntity>
{
    /// <summary>
    /// 使用原始SQL插入数据，用法：
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
    ICreated<TEntity> RawSql(string rawSql, object parameters);
    /// <summary>
    /// 使用插入对象部分栏位插入，可单个对象插入，也可以多个对象批量插入
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
    /// 批量插入,采用多表值方式，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObjs">插入对象，包含想要插入的必需栏位值</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> WithBy<TInsertObject>(TInsertObject insertObjs, int bulkCount = 500);
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
    ICreate<TEntity, TSource> From<TSource>(Expression<Func<TSource, object>> fieldSelector);
    ICreate<TEntity, T1, T2> From<T1, T2>(Expression<Func<T1, T2, object>> fieldSelector);
    ICreate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<T1, T2, T3, object>> fieldSelector);
    ICreate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object>> fieldSelector);
    ICreate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object>> fieldSelector);
}
public interface ICreated<TEntity>
{
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface ICreate<TEntity, TSource>
{
    ICreate<TEntity, TSource> Where(Expression<Func<TSource, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface ICreate<TEntity, T1, T2>
{
    ICreate<TEntity, T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface ICreate<TEntity, T1, T2, T3>
{
    ICreate<TEntity, T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface ICreate<TEntity, T1, T2, T3, T4>
{
    ICreate<TEntity, T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface ICreate<TEntity, T1, T2, T3, T4, T5>
{
    ICreate<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}