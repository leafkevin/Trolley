using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public interface ISqliteContinuedDelete<TEntity> : IContinuedDelete<TEntity>
{
    #region And
    /// <summary>
    /// 删除满足表达式predicate条件的数据，不局限于主键条件，predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    new ISqliteContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 删除满足表达式ifPredicate或elsePredicate条件的数据，不局限于主键条件，表达式ifPredicate不能为null。
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回删除对象</returns>
    new ISqliteContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    #endregion

    #region Returning
    /// <summary>
    /// 返回结果，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段名称列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回插入的部分字段</returns>
    ISqliteDeleted<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回结果，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    ISqliteDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}