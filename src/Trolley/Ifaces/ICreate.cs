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

    #region From
    /// <summary>
    /// 从表T中查询数据，进行插入，用法：
    /// <code>
    /// repository.Create&lt;TEntity&gt;()
    ///     .From&lt;T&gt;() 
    /// SQL:INSERT INTO ( ... ) SELECT ... FROM ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="suffixRawSql">额外的原始SQL, SqlServer会有With用法，如：<cdoe>SELECT * FROM sys_user WITH(NOLOCK)</cdoe>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> From<T>(string suffixRawSql = null);
    /// <summary>
    /// 使用2个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用3个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用4个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用5个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    /// <summary>
    /// 使用6个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    /// <summary>
    /// 使用7个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>();
    /// <summary>
    /// 使用8个表创建子查询对象，进行插入
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <typeparam name="T8">表T8实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>();
    /// <summary>
    /// 使用9个表创建子查询对象，进行插入
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
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
    /// <summary>
    /// 使用10个表创建子查询对象，进行插入
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
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>();
    #endregion

    #region UseIgnore/IfNotExists
    /// <summary>
    /// 相同主键或唯一索引存在时不执行插入动作，仅限MySql、Mariadb、PostgreSql数据库使用。MySql、Mariadb数据库，使用INSERT IGNORE INTO语句实现。PostgreSql数据库，使用INSERT INTO ... ON CONFLICT DO NOTHING语句实现。
    /// SqlServer数据库，请使用IfNotExists方法。
    /// </summary>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> UseIgnore();
    /// <summary>
    /// 相同主键或唯一索引存在时不执行插入动作。使用INSERT INTO ... SELECT ... WHERE NOT EXISTS(...)语句实现。MySql、Mariadb、PostgreSql数据库可使用UseIgnore方法更方便。
    /// </summary>
    /// <typeparam name="TFields">主键或是唯一索引键字段类型</typeparam>
    /// <param name="keys">主键或是唯一索引键字段值</param>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> IfNotExists<TFields>(TFields keys);
    /// <summary>
    /// 相同主键或唯一索引存在时不执行插入动作。使用INSERT INTO ... SELECT ... WHERE NOT EXISTS(...)语句实现。MySql、Mariadb、PostgreSql数据库可使用UseIgnore方法更方便。
    /// </summary>
    /// <param name="keysPredicate">主键或是唯一索引键值断言表达式</param>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> IfNotExists(Expression<Func<TEntity, bool>> keysPredicate);
    #endregion
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface ICreated<TEntity>
{
    #region OrUpdate
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，仅限SqlServer数据库使用。MySql、Mariadb数据库，使用INSERT INTO ... ON DUPLICATE KEY UPDATE语句实现。PostgreSql数据库，使用INSERT INTO ... ON CONFLICT DO NOTHING语句实现。
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="updateObj">更新对象</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> OrUpdate<TUpdateFields>(TUpdateFields updateObj);
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，仅限SqlServer数据库使用。MySql、Mariadb数据库，使用INSERT INTO ... ON DUPLICATE KEY UPDATE语句实现。PostgreSql数据库，使用INSERT INTO ... ON CONFLICT DO NOTHING语句实现。
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    ICreated<TEntity> OrUpdate<TUpdateFields>(Expression<Func<ICreateOrUpdate, TEntity, TUpdateFields>> fieldsAssignment);
    #endregion

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

    #region Execute
    MultipleCommand ToMultipleCommand();
    #endregion

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
public interface ICreateOrUpdate
{
    ICreateOrUpdate Alias(string aliasName);
    TField Values<TField>(TField fieldSelector);  
    TFields Set<TFields>(TFields updateObj);
    //TFields Set<TEntity, TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    //TFields Set<TEntity, TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
}