﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Trolley.MySqlConnector;

public class MySqlCreate<TEntity> : Create<TEntity>, IMySqlCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlCreate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as MySqlCreateVisitor;
    }
    #endregion

    #region IgnoreInto
    public IMySqlCreate<TEntity> IgnoreInto()
    {
        this.DialectVisitor.IsUseIgnoreInto = true;
        return this;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
    {
        base.WithBy(insertObj);
        return new MySqlContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulk
    public new IMySqlContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
    {
        base.WithBulk(insertObjs, bulkCount);
        return new MySqlContinuedCreate<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulkCopy
    public IMySqlCreated<TEntity> WithBulkCopy(IEnumerable<TEntity> insertObjs, int? timeoutSeconds = null)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        if (insertObjs is IDictionary<string, object>)
            throw new NotSupportedException("批量插入，单个对象类型只支持命名对象、匿名对象或是字典对象");

        this.DialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
        return new MySqlCreated<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion      
}
