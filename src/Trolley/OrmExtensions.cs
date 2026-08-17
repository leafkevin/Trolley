using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class OrmExtensions
{
    extension(IRepository repository)
    {
        #region QueryFirst
        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
        public TEntity QueryFirst<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
            => repository.From<TEntity>().Where(wherePredicate).First();
        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <param name="cancellationToken">取消Token</param>
        /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
        public async Task<TEntity> QueryFirstAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
            => await repository.From<TEntity>().Where(wherePredicate).FirstAsync(cancellationToken);
        #endregion

        #region Query
        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
        public List<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
            => repository.From<TEntity>().Where(wherePredicate).ToList();
        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录，记录不存在时返回没有任何元素的空列表
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <param name="cancellationToken">取消Token</param>
        /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
        public async Task<List<TEntity>> QueryAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
            => await repository.From<TEntity>().Where(wherePredicate).ToListAsync(cancellationToken);
        #endregion

        #region Update
        /// <summary>
        /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如：
        /// <code>
        /// var orderInfo = new OrderInfo { ... };
        /// var tmpObj = new { TotalAmount = 450, ... };
        /// repository.Update&lt;Order&gt;(f => new
        /// {
        ///     parameter.TotalAmount, //直接赋值，使用同名变量
        ///     Products = repository.GetProducts(), //直接赋值，使用本地函数
        ///     BuyerId = DBNull.Value, //直接赋值 NULL
        ///     Disputes = new Dispute { ... } //使用updateObjs对象中的参数，实体对象由TypeHandler处理
        /// }, x =&gt; x.Id == 1);
        /// private int[] GetProducts() => new int[] { 1, 2, 3 };
        /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
        /// </code>
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
        /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不可为null</param>
        /// <returns>返回更新行数</returns>
        public int Update<TEntity>(Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate)
            => repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).Execute();
        /// <summary>
        /// 使用表达式fieldsAssignment部分字段更新，表达式fieldsAssignment的字段可以是一个或是多个，如：
        /// <code>
        /// var orderInfo = new OrderInfo { ... };
        /// var tmpObj = new { TotalAmount = 450, ... };
        /// await repository.UpdateAsync&lt;Order&gt;(f => new
        /// {
        ///     parameter.TotalAmount, //直接赋值，使用同名变量
        ///     Products = repository.GetProducts(), //直接赋值，使用本地函数
        ///     BuyerId = DBNull.Value, //直接赋值 NULL
        ///     Disputes = new Dispute { ... } //使用updateObjs对象中的参数，实体对象由TypeHandler处理
        /// }, x =&gt; x.Id == 1);
        /// private int[] GetProducts() => new int[] { 1, 2, 3 };
        /// SQL: UPDATE `sys_order` SET `TotalAmount`=@TotalAmount,`Products`=@Products,`BuyerId`=NULL,`Disputes`=@Disputes WHERE `Id`=1
        /// </code>
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="fieldsAssignment">更新字段表达式，一个或是多个字段成员访问表达式，同名字段省略赋值字段，如：parameter.TotalAmount</param>
        /// <param name="wherePredicate">条件表达式，条件表达式wherePredicate不可为null</param>
        /// <param name="cancellationToken">取消Token</param>
        /// <returns>返回更新行数</returns>
        public async Task<int> UpdateAsync<TEntity>(Expression<Func<TEntity, object>> fieldsAssignment, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
            => await repository.Update<TEntity>().Set(fieldsAssignment).Where(wherePredicate).ExecuteAsync(cancellationToken);
        #endregion

        #region Delete
        /// <summary>
        /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不可为null
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="repository">仓储对象</param>
        /// <param name="wherePredicate">条件表达式，表达式predicate不可为null</param>
        /// <returns>返回删除行数</returns>
        public int Delete<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
            => repository.Delete<TEntity>().Where(wherePredicate).Execute();
        /// <summary>
        /// 删除满足表达式wherePredicate条件的数据，不局限于主键条件，表达式wherePredicate不可为null
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="repository">仓储对象</param>
        /// <param name="wherePredicate">条件表达式，表达式predicate不可为null</param>
        /// <param name="cancellationToken">取消Token</param>
        /// <returns>返回删除行数</returns>
        public async Task<int> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
            => await repository.Delete<TEntity>().Where(wherePredicate).ExecuteAsync(cancellationToken);
        #endregion
    }

    extension(IMultipleQuery instance)
    {
        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的第一条记录，条件表达式wherePredicate可以为null，为null时，查询所有记录的第一条
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <returns>返回多语句查询对象，将TEntity类型值添加到Reader中</returns>
        public IMultipleQuery QueryFirst<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
            => instance.From<TEntity>().Where(wherePredicate).First();

        /// <summary>
        /// 查询TEntity实体表满足表达式wherePredicate条件的所有记录，条件表达式wherePredicate可以为null，为null时，查询所有记录
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="wherePredicate">条件表达式</param>
        /// <returns>返回多语句查询对象，将TEntity类型列表添加到Reader中</returns>
        public IMultipleQuery Query<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
            => instance.From<TEntity>().Where(wherePredicate).ToList();
    }
}