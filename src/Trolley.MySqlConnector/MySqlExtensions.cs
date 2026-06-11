using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    extension<TInsertObj>(TInsertObj instance)
    {
        /// <summary>
        /// 获取插入字段原值
        /// </summary>
        /// <typeparam name="TField">插入字段类型</typeparam>
        /// <param name="field">插入字段值</param>
        /// <returns>插入对象原值</returns>
        /// <exception cref="NotImplementedException"></exception>
        public TField Values<TField>(TField field) => throw new NotImplementedException();
    }
    extension(ICreate instance)
    {
        public ICreate IgnoreInto()
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.IsUseIgnoreInto = true;
            return instance;
        }
        public IBulkContinuedCreate WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null)
        {
            if (insertObjs == null)
                throw new ArgumentNullException(nameof(insertObjs));
            bool isEmpty = true;
            foreach (var insertObj in insertObjs)
            {
                isEmpty = false;
                break;
            }
            if (isEmpty) throw new Exception("批量更新，insertObjs参数至少要有一条数据");
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
            return dialectVisitor.OrmProvider.NewBulkContinuedCreate(instance.DbContext, dialectVisitor);
        }
    }

    extension<TEntity>(ICreate<TEntity> instance)
    {
        public ICreate<TEntity> IgnoreInto()
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.IsUseIgnoreInto = true;
            return instance;
        }
        public IBulkContinuedCreate<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null)
        {
            if (insertObjs == null)
                throw new ArgumentNullException(nameof(insertObjs));
            bool isEmpty = true;
            foreach (var insertObj in insertObjs)
            {
                isEmpty = false;
                break;
            }
            if (isEmpty) throw new Exception("批量更新，insertObjs参数至少要有一条数据");
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
            return dialectVisitor.OrmProvider.NewBulkContinuedCreate<TEntity>(instance.DbContext, dialectVisitor);
        }
    }
    extension(IContinuedCreate instance)
    {
        public IMySqlCreateDuplicateKeyUpdate OnDuplicateKeyUpdate()
            => new MySqlCreateDuplicateKeyUpdate(instance.DbContext, instance.Visitor);
        public IResultCommand<TResult> Returning<TResult>(string fieldNames)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldNames);
            return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension<TEntity>(IContinuedCreate<TEntity> instance)
    {
        public IMySqlCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate()
            => new MySqlCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, instance.Visitor);
        public IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldsSelector);
            return dialectVisitor.OrmProvider.NewResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension(IBulkContinuedCreate instance)
    {
        public IMySqlBulkCreateDuplicateKeyUpdate OnDuplicateKeyUpdate()
            => new MySqlBulkCreateDuplicateKeyUpdate(instance.DbContext, instance.Visitor);
        public IBulkResultCommand<TResult> Returning<TResult>(string fieldNames)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldNames);
            return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension<TEntity>(IBulkContinuedCreate<TEntity> instance)
    {
        public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate()
            => new MySqlBulkCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, instance.Visitor);
        public IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldsSelector);
            return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension(IFromCreate instance)
    {
        public IFromCreate IgnoreInto()
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.IsUseIgnoreInto = true;
            return instance;
        }
        public IMySqlBulkCreateDuplicateKeyUpdate OnDuplicateKeyUpdate()
            => new MySqlBulkCreateDuplicateKeyUpdate(instance.DbContext, instance.Visitor);
        public IBulkResultCommand<TResult> Returning<TResult>(string fieldNames)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldNames);
            return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension<TEntity>(IFromCreate<TEntity> instance)
    {
        public IFromCreate<TEntity> IgnoreInto()
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.IsUseIgnoreInto = true;
            return instance;
        }
        public IMySqlBulkCreateDuplicateKeyUpdate<TEntity> OnDuplicateKeyUpdate()
            => new MySqlBulkCreateDuplicateKeyUpdate<TEntity>(instance.DbContext, instance.Visitor);
        public IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
        {
            var dialectVisitor = instance.Visitor as MySqlCreateVisitor;
            dialectVisitor.Returning(fieldsSelector);
            return dialectVisitor.OrmProvider.NewBulkResultCreated<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension(IUpdate instance)
    {
        public IBulkContinuedUpdate SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null)
        {
            if (updateObjs == null)
                throw new ArgumentNullException(nameof(updateObjs));

            if (updateObjs is IDictionary<string, object>)
                throw new NotSupportedException("批量更新，单个对象类型只支持命名对象、匿名对象或是字典对象");

            bool isEmpty = true;
            foreach (var updateObj in updateObjs)
            {
                isEmpty = false;
                break;
            }
            if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var dialectVisitor = instance.Visitor as MySqlUpdateVisitor;
            dialectVisitor.SetBulkCopy(updateObjs, timeoutSeconds);
            return dialectVisitor.OrmProvider.NewBulkContinuedUpdate(instance.DbContext, dialectVisitor);
        }
    }
    extension<TEntity>(IUpdate<TEntity> instance)
    {
        public IBulkContinuedUpdate<TEntity> SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null)
        {
            if (updateObjs == null)
                throw new ArgumentNullException(nameof(updateObjs));

            if (updateObjs is IDictionary<string, object>)
                throw new NotSupportedException("批量更新，单个对象类型只支持命名对象、匿名对象或是字典对象");

            bool isEmpty = true;
            foreach (var updateObj in updateObjs)
            {
                isEmpty = false;
                break;
            }
            if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var dialectVisitor = instance.Visitor as MySqlUpdateVisitor;
            dialectVisitor.SetBulkCopy(updateObjs, timeoutSeconds);
            return dialectVisitor.OrmProvider.NewBulkContinuedUpdate<TEntity>(instance.DbContext, dialectVisitor);
        }
    }
    extension(IDelete instance)
    {
        public IBulkResultCommand<TResult> Returning<TResult>(string fieldNames)
        {
            var dialectVisitor = instance.Visitor as MySqlDeleteVisitor;
            dialectVisitor.Returning(fieldNames);
            return dialectVisitor.OrmProvider.NewResultDeleted<TResult>(instance.DbContext, instance.Visitor);
        }
    }
    extension<TEntity>(IDelete<TEntity> instance)
    {
        public IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
        {
            var dialectVisitor = instance.Visitor as MySqlDeleteVisitor;
            dialectVisitor.Returning(fieldsSelector);
            return dialectVisitor.OrmProvider.NewResultDeleted<TResult>(instance.DbContext, instance.Visitor);
        }
    }
}