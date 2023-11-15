using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface ICreate<TEntity>
{
    #region Properties
    ICreateVisitor Visitor { get; }
    #endregion

    #region WithBy
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
    IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    #endregion

    #region WithBulk
    /// <summary>
    /// 批量插入,采用多表值方式，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name],[BrandId],[CategoryId],[IsEnabled],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (@ProductNo0,@Name0,@BrandId0,@CategoryId0,@IsEnabled0,@CreatedAt0,@CreatedBy0,@UpdatedAt0,@UpdatedBy0),(@ProductNo1,@Name1,@BrandId1,@CategoryId1,@IsEnabled1,@CreatedAt1,@CreatedBy1,@UpdatedAt1,@UpdatedBy1),(@ProductNo2,@Name2,@BrandId2,@CategoryId2,@IsEnabled2,@CreatedAt2,@CreatedBy2,@UpdatedAt2,@UpdatedBy2)
    /// </code>
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500);
    #endregion 

    //#region IfNotExists   
    ///// <summary>
    ///// 相同主键或唯一索引存在时不执行插入动作。使用INSERT INTO ... SELECT ... WHERE NOT EXISTS(...)语句实现。MySql、Mariadb、PostgreSql数据库可使用UseIgnore方法更方便。
    ///// </summary>
    ///// <typeparam name="TFields">主键或是唯一索引键字段类型</typeparam>
    ///// <param name="keys">主键或是唯一索引键字段值</param>
    ///// <returns>返回插入对象</returns>
    //ICreate<TEntity> IfNotExists<TFields>(TFields keys);
    ///// <summary>
    ///// 相同主键或唯一索引存在时不执行插入动作。使用INSERT INTO ... SELECT ... WHERE NOT EXISTS(...)语句实现。MySql、Mariadb、PostgreSql数据库可使用UseIgnore方法更方便。
    ///// </summary>
    ///// <param name="keysPredicate">主键或是唯一索引键值断言表达式</param>
    ///// <returns>返回插入对象</returns>
    //ICreate<TEntity> IfNotExists(Expression<Func<TEntity, bool>> keysPredicate);
    //#endregion
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface ICreated<TEntity>
{
    #region Execute
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    int Execute();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    long ExecuteLong();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    Task<long> ExecuteLongAsync(CancellationToken cancellationToken = default);
    #endregion

    //#region Execute
    //MultipleCommand ToMultipleCommand();
    //#endregion

    #region ToSql
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
    #endregion
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface IContinuedCreate<TEntity> : ICreated<TEntity>
{
    #region Properties
    ICreateVisitor Visitor { get; }
    #endregion

    #region WithBy
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，可多次调用，自动增长的栏位，不需要传入，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "kevin",
    ///         Age = 25
    ///     })
    ///     .WithBy(true, new
    ///     {
    ///         ...
    ///     })
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`, ... ) VALUES(@Name,@Age, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入数据对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用插入对象部分字段插入，单个对象插入，可多次调用，自动增长的栏位，不需要传入，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "kevin",
    ///         Age = 25
    ///     })
    ///     .WithBy(true, new
    ///     {
    ///         ...
    ///     })
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`, ... ) VALUES(@Name,@Age, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="insertObj">插入数据对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用fieldValue单个字段插入，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "kevin",
    ///         Age = 25
    ///     })
    ///     .WithBy(true, f =&gt; f.Gender, Gender.Female)
    ///     ...
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`Gender`, ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion
}