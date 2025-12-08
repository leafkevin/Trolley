using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public interface ISqlServerContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    #region WithBy
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { ... })
    ///     .WithBy(new { Name = "kevin", Age = 25 }) ...
    /// SQL: INSERT INTO [sys_user] ( ..., [Name],[Age], ... ) VALUES(..., @Name,@Age, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，condition为true生效，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, new { Gender = Gender.Male, ... })
    ///     .Execute();
    /// SQL: INSERT INTO [sys_user] ([Name],[Age],[Gender], ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用fieldValue单个字段插入，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, f =&gt; f.Gender, Gender.Female)
    ///     ...
    ///     .Execute();
    /// SQL: INSERT INTO [sys_user] ([Name],[Age],[Gender], ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 忽略字段，实体属性名称，列出属性不插入，如：.IgnoreFields("Name") 或是 .IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，列出属性不插入，如：.IgnoreFields(f =&gt; f.Name) 或是 .IgnoreFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 插入字段，实体属性名称，未列出属性不插入，如：.OnlyFields("Name") 或是 .OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 插入字段，未列出属性不插入，如：.OnlyFields(f =&gt; f.Name) 或是 .OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region Output
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerCreated<TEntity, TResult> Output<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}
public interface ISqlServerBulkContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    #region WithBy
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { ... })
    ///     .WithBy(new { Name = "kevin", Age = 25 }) ...
    /// SQL: INSERT INTO [sys_user] ( ..., [Name],[Age], ... ) VALUES(..., @Name,@Age, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，condition为true生效，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, new { Gender = Gender.Male, ... })
    ///     .Execute();
    /// SQL: INSERT INTO [sys_user] ([Name],[Age],[Gender], ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用fieldValue单个字段插入，如：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, f =&gt; f.Gender, Gender.Female)
    ///     ...
    ///     .Execute();
    /// SQL: INSERT INTO [sys_user] ([Name],[Age],[Gender], ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 忽略字段，实体属性名称，列出属性不插入，如：.IgnoreFields("Name") 或是 .IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，列出属性不插入，如：.IgnoreFields(f =&gt; f.Name) 或是 .IgnoreFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 插入字段，实体属性名称，未列出属性不插入，如：.OnlyFields("Name") 或是 .OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 插入字段，未列出属性不插入，如：.OnlyFields(f =&gt; f.Name) 或是 .OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new ISqlServerBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region Output
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}