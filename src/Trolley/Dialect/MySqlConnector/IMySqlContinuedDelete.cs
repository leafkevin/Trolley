using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlContinuedDelete<TEntity> : IContinuedDelete<TEntity>
{
    #region And
    /// <summary>
    /// 使用predicate表达式生成And条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> And(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成And条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成And条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成And条件，predicateInitializer不能为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不能为null</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> AndPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> Or(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Or条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Or条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> Or(bool condition, Expression<Func<TEntity, bool>> ifPredicate = null, Expression<Func<TEntity, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Or条件，predicateInitializer不能为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不能为null</param>
    /// <returns>返回删除对象</returns>
    new IMySqlContinuedDelete<TEntity> OrPredicate(Func<PredicateBuilder<TEntity>, Expression<Func<TEntity, bool>>> predicateInitializer);
    #endregion

    #region Returning
    /// <summary>
    /// 返回结果，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段名称列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回已删除的部分字段值</returns>
    IMySqlDeleted<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回结果，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回已删除的部分字段值</returns>
    IMySqlDeleted<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}