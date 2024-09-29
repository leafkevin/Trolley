using System;
using System.Collections;
using System.Collections.Generic;

namespace Trolley.SqlServer;

public class SqlServerCreate<TEntity> : Create<TEntity>, ISqlServerCreate<TEntity>
{
    #region Properties
    public SqlServerCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqlServerCreate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as SqlServerCreateVisitor;
    }
    #endregion

    #region Sharding
    public new ISqlServerCreate<TEntity> UseTable(string tableName)
        => base.UseTable(tableName) as ISqlServerCreate<TEntity>;
    public new ISqlServerCreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as ISqlServerCreate<TEntity>;
    #endregion

    #region WithLock
    public ISqlServerCreate<TEntity> WithLock(string lockName)
    {
        this.DialectVisitor.WithLock(lockName);
        return this;
    }
    #endregion

    #region WithBy
    public new ISqlServerContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => base.WithBy(insertObj) as ISqlServerContinuedCreate<TEntity>;
    #endregion

    #region WithBulk
    public new ISqlServerBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
        => base.WithBulk(insertObjs, bulkCount) as ISqlServerBulkContinuedCreate<TEntity>;
    #endregion

    #region WithBulkCopy
    public ISqlServerCreated<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");

        bool isEmpty = true;
        foreach (var insertObj in insertObjs)
        {
            isEmpty = false;
            break;
        }
        if (isEmpty) throw new Exception("批量更新，insertObjs参数至少要有一条数据");

        this.DialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
        return this.OrmProvider.NewCreated<TEntity>(this.DbContext, this.Visitor) as ISqlServerCreated<TEntity>;
    }
    #endregion
}