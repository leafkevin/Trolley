using MySqlConnector;
using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static MemberBuilder<TMember> NativeDbType<TMember>(this MemberBuilder<TMember> builder, MySqlDbType nativeDbType)
       => builder.NativeDbType(nativeDbType);

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
