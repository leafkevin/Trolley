using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IUpdate<TEntity>
{
    IUpdateSet<TEntity> WithBy<TFields>(TFields parameters);
    IUpdateSet<TEntity> WithBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, object parameters);

    IUpdateSet<TEntity> WithBulkBy<TFields>(TFields parameters, int bulkCount = 500);
    IUpdateSet<TEntity> WithBulkBy<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr, object parameters, int bulkCount = 500);

    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    IUpdateFrom<TEntity, T> From<T>();
    IUpdateFrom<TEntity, T1, T2> From<T1, T2>();
    IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>();
    IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();

    IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn);
    IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn);
}
public interface IUpdateSet<TEntity>
{
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IUpdateSetting<TEntity> : IUpdateSet<TEntity>
{
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsExpr);
    IUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateSetting<TEntity> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateSetting<TEntity> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateSetting<TEntity> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateFrom<TEntity, T1> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion 
}
public interface IUpdateJoin<TEntity, T1> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn);

    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate = null, Expression<Func<TEntity, T1, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateFrom<TEntity, T1, T2> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion 
}
public interface IUpdateJoin<TEntity, T1, T2> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateFrom<TEntity, T1, T2, T3> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion 
}
public interface IUpdateJoin<TEntity, T1, T2, T3> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion 
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn);

    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateFrom<TEntity, T1, T2, T3, T4, T5> : IUpdateSet<TEntity>
{
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}
public interface IUpdateJoin<TEntity, T1, T2, T3, T4, T5> : IUpdateSet<TEntity>
{
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> fieldValueExpr);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValue<TField>(Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetValueIf<TField>(bool condition, Expression<Func<TEntity, TField>> fieldExpr, TField fieldValue);

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回更新对象</returns>
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion
}