using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class OrmExtensions
{
    #region QueryFirst/Query/QueryDictionary
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    public static TEntity QueryFirst<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.First();
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.FirstAsync(cancellationToken);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    public static List<TEntity> Query<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.ToList();
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.ToListAsync(cancellationToken);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，返回TEntity实体所有字段的记录并转化为Dictionary&lt;TKey, TValue&gt;字典，记录不存在时返回没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">字典Key类型</typeparam>
    /// <typeparam name="TValue">字典Value类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate可以为null，为null时，查询所有记录</param>
    /// <param name="keySelector">字典Key选择委托</param>
    /// <param name="valueSelector">字典Value选择委托</param>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;字典或没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典</returns>
    public static Dictionary<TKey, TValue> QueryDictionary<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector) where TKey : notnull
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return query.ToDictionary(keySelector, valueSelector);
    }
    /// <summary>
    /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，返回TEntity实体所有字段的记录并转化为Dictionary&lt;TKey, TValue&gt;字典，记录不存在时返回没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典，条件表达式wherePredicate可以为null，为null时，查询所有记录
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">字典Key类型</typeparam>
    /// <typeparam name="TValue">字典Value类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate可以为null，为null时，查询所有记录</param>
    /// <param name="keySelector">字典Key选择委托</param>
    /// <param name="valueSelector">字典Value选择委托</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;字典或没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典</returns>
    public static async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TEntity, TKey, TValue>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        var query = repository.From<TEntity>();
        if (wherePredicate != null)
            query.Where(wherePredicate);
        return await query.ToDictionaryAsync(keySelector, valueSelector, cancellationToken);
    }
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
    /// <typeparam name="TEntity">实体类型，需要有对应的模型映射找到要插入的表</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="insertObj">插入对象，匿名对象或是实体对象，不支持某个变量值，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <returns>返回插入行数</returns>
    #endregion

    #region Create
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，自动增长栏位，不需要传入，用法：
    /// <code>
    /// repository.Create&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     CompanyId = 1,
    ///     Gender = Gender.Male,
    ///     IsEnabled = true,
    ///     CreatedAt = DateTime.Now,
    ///     CreatedBy = 1,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_user` (`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="insertObj">插入对象，匿名对象或是实体对象，不支持某个变量值，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <returns>返回插入行数</returns>
    public static int Create<TEntity>(this IRepository repository, object insertObj)
        => repository.Create<TEntity>().WithBy(insertObj).Execute();
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，自动增长栏位，不需要传入，用法：
    /// <code>
    /// await repository.CreateAsync&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     CompanyId = 1,
    ///     Gender = Gender.Male,
    ///     IsEnabled = true,
    ///     CreatedAt = DateTime.Now,
    ///     CreatedBy = 1,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// INSERT INTO `sys_user` (`Name`,`Age`,`CompanyId`,`Gender`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@CompanyId,@Gender,@IsEnabled,@CreatedAt,@CreatedBy,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="insertObj">插入对象，匿名对象或是实体对象，不支持某个变量值，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, object insertObj, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithBy(insertObj).ExecuteAsync(cancellationToken);
    /// <summary>
    /// 使用原始SQL和参数插入数据，如：repository.Create&lt;Order&gt;("INSERT INTO Table(Field1,Field2) VALUES(@Value1,@Value2)", new { Value1 = 1, Value2 = "xxx" });
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameter">SQL中使用的参数，匿名对象或是实体对象，不支持某个变量值，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <returns>返回插入行数</returns>
    public static int Create<TEntity>(this IRepository repository, string rawSql, object parameter)
        => repository.Create<TEntity>().RawSql(rawSql, parameter).Execute();
    /// <summary>
    /// 使用原始SQL和参数插入数据，如：repository.CreateAsync&lt;Order&gt;("INSERT INTO Table(Field1,Field2) VALUES(@Value1,@Value2)", new { Value1 = 1, Value2 = "xxx" });
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameter">SQL中使用的参数，匿名对象或是实体对象，不支持某个变量值，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, string rawSql, object parameter, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().RawSql(rawSql, parameter).ExecuteAsync(cancellationToken);
    /// <summary>
    /// 批量插入,采用多表值方式，insertObjs可以是匿名对象、实体对象、字典等集合，不能是单个元素，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="insertObjs">插入的对象集合，可以是匿名对象、实体对象字典等集合，不能是单个元素</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入行数</returns>
    public static int Create<TEntity>(this IRepository repository, IEnumerable insertObjs, int bulkCount = 500)
        => repository.Create<TEntity>().WithBulk(insertObjs, bulkCount).Execute();
    /// <summary>
    /// 批量插入,采用多表值方式，insertObjs可以是匿名对象、实体对象、字典等集合，不能是单个元素，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="insertObjs">插入的对象集合，可以是匿名对象、实体对象字典等集合，不能是单个元素</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, IEnumerable insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithBulk(insertObjs, bulkCount).ExecuteAsync(cancellationToken);
    #endregion

    #region Update
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;User&gt;(f =&gt; new { SomeTimes = TimeSpan.FromMinutes(1455) }, x =&gt; x.Id == 1);
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;(f => new
    /// {
    ///     parameter.TotalAmount,
    ///     Products = this.GetProducts(),
    ///     BuyerId = DBNull.Value,
    ///     Disputes = new Dispute
    ///     {
    ///         Id = 1,
    ///         Content = "43dss",
    ///         Users = "1,2",
    ///         Result = "OK",
    ///         CreatedAt = DateTime.Now
    ///     }
    /// }, x =&gt; x.Id == 1);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SQL1:UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=1
    /// SQL2:UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不能为null</param>
    /// <returns>返回更新行数</returns>
    public static int Update<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate)
        => repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).Execute();
    /// <summary>
    /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，用法：
    /// <code>
    /// repository.Update&lt;User&gt;(f =&gt; new { SomeTimes = TimeSpan.FromMinutes(1455) }, x =&gt; x.Id == 1);
    /// var parameter = repository.Get&lt;Order&gt;(1);
    /// parameter.TotalAmount += 50;
    /// repository.Update&lt;Order&gt;(f => new
    /// {
    ///     parameter.TotalAmount,
    ///     Products = this.GetProducts(),
    ///     BuyerId = DBNull.Value,
    ///     Disputes = new Dispute
    ///     {
    ///         Id = 1,
    ///         Content = "43dss",
    ///         Users = "1,2",
    ///         Result = "OK",
    ///         CreatedAt = DateTime.Now
    ///     }
    /// }, x =&gt; x.Id == 1);
    /// private int[] GetProducts() => new int[] { 1, 2, 3 };
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SQL1:UPDATE `sys_user` SET `SomeTimes`=@SomeTimes WHERE `Id`=1
    /// SQL2:UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
    /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).ExecuteAsync(cancellationToken);
    /// <summary>
    /// 使用集合对象updateObjs部分字段批量更新，集合对象updateObjs中的单个元素实体中必须包含主键字段，支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，用法：
    /// <code>
    /// var parameters = new []{ new { Id = 1, Name = "Name1" }, new { Id = 2, Name = "Name2" }};
    /// repository.Update&lt;User&gt;(parameters);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@kId1;
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="updateObjs">更新对象参数集合，包含想要更新的必需栏位和主键字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    public static int Update<TEntity>(this IRepository repository, IEnumerable updateObjs, int bulkCount = 500)
        => repository.Update<TEntity>().WithBulk(updateObjs, bulkCount).Execute();
    /// <summary>
    /// 使用集合对象updateObjs部分字段批量更新，集合对象updateObjs中的单个元素实体中必须包含主键字段，支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，用法：
    /// <code>
    /// var parameters = new []{ new { Id = 1, Name = "Name1" }, new { Id = 2, Name = "Name2" }};
    /// await repository.UpdateAsync&lt;User&gt;(parameters);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@kId0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@kId1;
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="updateObjs">更新对象参数集合，包含想要更新的必需栏位和主键字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新对象</returns>
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, IEnumerable updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBulk(updateObjs, bulkCount).ExecuteAsync(cancellationToken);
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和集合对象updateObjs部分字段批量更新，集合对象updateObjs中的单个元素实体中必须包含主键字段，支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，用法：
    /// <code>
    /// var orders = await repository.From&lt;Order&gt;()
    ///     .Where(f =&gt; new int[] { 1, 2, 3 }.Contains(f.Id))
    ///     .ToListAsync();
    /// repository.Update&lt;Order&gt;(f =&gt; new
    /// {
    ///     BuyerId = DBNull.Value,
    ///     OrderNo = "ON_" + f.OrderNo,
    ///     f.TotalAmount
    /// }, orders);
    /// </code>   
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2
    /// </code>
    /// 执行后的结果，栏位BuyerId将被更新为固定值NULL，栏位OrderNo将被更新为ON_+数据库中原值，栏位TotalAmount将被更新为参数orders中提供的值
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObjs">更新对象参数集合，包含想要更新的必需栏位和主键字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <returns>返回更新行数</returns>
    public static int Update<TEntity>(this IRepository repository, Expression<Func<TEntity, object>> fieldsSelectorOrAssignment, IEnumerable updateObjs, int bulkCount = 500)
        => repository.Update<TEntity>().WithBulk(fieldsSelectorOrAssignment, updateObjs, bulkCount).Execute();
    /// <summary>
    /// 使用表达式fieldsSelectorOrAssignment字段筛选和集合对象updateObjs部分字段批量更新，集合对象updateObjs中的单个元素实体中必须包含主键字段，支持分批次更新，更新条数超过设置的bulkCount值，将在下次更新，直到所有数据更新完毕，bulkCount默认500，用法：
    /// <code>
    /// var orders = await repository.From&lt;Order&gt;()
    ///     .Where(f =&gt; new int[] { 1, 2, 3 }.Contains(f.Id))
    ///     .ToListAsync();
    /// repository.Update&lt;Order&gt;(f =&gt; new
    /// {
    ///     BuyerId = DBNull.Value,
    ///     OrderNo = "ON_" + f.OrderNo,
    ///     f.TotalAmount
    /// }, orders);
    /// </code>   
    /// 生成的SQL:
    /// <code>
    /// UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2
    /// </code>
    /// 执行后的结果，栏位BuyerId将被更新为固定值NULL，栏位OrderNo将被更新为ON_+数据库中原值，栏位TotalAmount将被更新为参数orders中提供的值
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="fieldsSelectorOrAssignment">字段筛选表达式，既可以筛选字段，也可以用表达式的值更新字段，只有带参数的成员访问的表达式(如：f =&gt; f.Name)，才会被更新为updateObj中对应栏位的值，其他场景将被更新为对应表达式的值(如：tmpObj.TotalAmount, BuyerId = DBNull.Value等)</param>
    /// <param name="updateObjs">更新对象参数集合，包含想要更新的必需栏位和主键字段</param>
    /// <param name="bulkCount">单次更新的最大数据条数，默认是500</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    public static async Task<int> UpdateAsync<TEntity, TFields>(this IRepository repository, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, IEnumerable updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBulk(fieldsSelectorOrAssignment, updateObjs, bulkCount).ExecuteAsync(cancellationToken);
    #endregion

    #region Delete
    /// <summary>
    /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除行数</returns>
    public static int Delete<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate)
        => repository.Delete<TEntity>().Where(wherePredicate).Execute();
    /// <summary>
    /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="wherePredicate">条件表达式，表达式predicate不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(wherePredicate).ExecuteAsync(cancellationToken);
    /// <summary>
    /// 根据主键删除数据，可以删除一条也可以删除多条记录，keys可以是主键值也可以是包含主键值的匿名对象，用法：
    /// <code>
    /// 单个删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(1);
    /// repository.Delete&lt;User&gt;(new { Id = 1});
    /// 批量删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(new[] { 1, 2 });
    /// repository.Delete&lt;User&gt;(new[] { new { Id = 1 }, new { Id = 2 } });
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="keys">主键值，可以是一个值或是一个匿名对象，也可以是多个值或是多个匿名对象</param>
    /// <returns>返回删除行数</returns>
    public static int Delete<TEntity>(this IRepository repository, object keys)
        => repository.Delete<TEntity>().Where(keys).Execute();
    /// <summary>
    /// 根据主键删除数据，可以删除一条也可以删除多条记录，keys可以是主键值也可以是包含主键值的匿名对象，用法：
    /// <code>
    /// 单个删除,下面两个方法等效
    /// await repository.DeleteAsync&lt;User&gt;(1);
    /// await repository.DeleteAsync&lt;User&gt;(new { Id = 1});
    /// 批量删除,下面两个方法等效
    /// await repository.DeleteAsync&lt;User&gt;(new[] { 1, 2 });
    /// await repository.DeleteAsync&lt;User&gt;(new[] { new { Id = 1 }, new { Id = 2 } });
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="repository">仓储对象</param>
    /// <param name="keys">主键值，可以是一个值或是一个匿名对象，也可以是多个值或是多个匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, object keys, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(keys).ExecuteAsync(cancellationToken);
    #endregion 
}