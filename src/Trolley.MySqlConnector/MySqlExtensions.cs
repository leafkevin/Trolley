using MySqlConnector;
using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, MySqlDbType nativeDbType)
       => builder.NativeDbType(nativeDbType);

    /// <summary>
    /// 使用cte子句cteSubQuery插入数据，如果cteSubQuery子句中包含UnionRecursive/UnionAllRecursive方法调用，无需设置cteTableName
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    /// <param name="creater"></param>
    /// <param name="cteSubQuery"></param>
    /// <param name="cteTableName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    public static ICreated<TEntity> WithFrom<TEntity, TTarget>(this ICreate<TEntity> creater, Func<IFromQuery, IQuery<TTarget>> cteSubQuery, string cteTableName = null)
    {
        if (creater == null)
            throw new ArgumentNullException(nameof(creater));
        if (cteSubQuery == null)
            throw new ArgumentNullException(nameof(cteSubQuery));
        if (creater.Visitor is not MySqlCreateVisitor mySqlVisitor)
            throw new NotSupportedException("只有MySql/Mariadb数据库才支持此接口");
        mySqlVisitor.WithFrom(cteSubQuery, cteTableName);
        return new Created<TEntity>(creater.DbContext, mySqlVisitor);
    }
    /// <summary>
    /// 相同主键或唯一索引存在时不执行插入动作，INSERT IGNORE INTO...
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="creater">插入对象</param>
    /// <returns>返回插入对象</returns>
    /// <exception cref="ArgumentNullException">插入对象为null时报错</exception>
    /// <exception cref="NotSupportedException">除MySql、MariaDB数据库外都报不支持此操作错误</exception>
    public static ICreate<TEntity> IgnoreInto<TEntity>(this ICreate<TEntity> creater)
    {
        if (creater == null)
            throw new ArgumentNullException(nameof(creater));
        if (creater.Visitor is not MySqlCreateVisitor mySqlVisitor)
            throw new NotSupportedException("只有MySql/Mariadb数据库才支持此接口");
        mySqlVisitor.IsUseIgnoreInto = true;
        return creater;
    }
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="creater">插入对象</param>
    /// <param name="updateObj">更新实体对象</param>
    /// <returns>返回插入对象</returns>
    /// <exception cref="ArgumentNullException">插入对象为null时报错</exception>
    /// <exception cref="NotSupportedException">除MySql、MariaDB数据库外都报不支持此操作错误</exception>
    public static IContinuedCreate<TEntity> OnDuplicateKeyUpdate<TEntity, TUpdateFields>(this IContinuedCreate<TEntity> creater, TUpdateFields updateObj)
    {
        if (creater == null)
            throw new ArgumentNullException(nameof(creater));
        if (creater.Visitor is not MySqlCreateVisitor mySqlVisitor)
            throw new NotSupportedException("只有MySql/Mariadb数据库才支持此接口");
        mySqlVisitor.OnDuplicateKeyUpdate(updateObj);
        return creater;
    }
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="creater">插入对象</param>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    /// <exception cref="ArgumentNullException">插入对象为null时报错</exception>
    /// <exception cref="NotSupportedException">除MySql、MariaDB数据库外都报不支持此操作错误</exception>
    public static IContinuedCreate<TEntity> OnDuplicateKeyUpdate<TEntity, TUpdateFields>(this IContinuedCreate<TEntity> creater, Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)
    {
        if (creater == null)
            throw new ArgumentNullException(nameof(creater));
        if (creater.Visitor is not MySqlCreateVisitor mySqlVisitor)
            throw new NotSupportedException("只有MySql/Mariadb数据库才支持此接口");
        mySqlVisitor.OnDuplicateKeyUpdate(fieldsAssignment);
        return creater;
    }
}
