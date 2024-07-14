using System;
using System.Collections;
using System.Collections.Generic;

namespace Trolley.PostgreSql;

public class PostgreSqlCreate<TEntity> : Create<TEntity>, IPostgreSqlCreate<TEntity>
{
    #region Properties
    public PostgreSqlCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlCreate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlCreateVisitor;
    }
    #endregion

    #region Sharding
    public override IPostgreSqlCreate<TEntity> UseTable(string tableName)
    {
        base.UseTable(tableName);
        return this;
    }
    public override IPostgreSqlCreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    #endregion

    #region WithBy
    public override IPostgreSqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => base.WithBy(insertObj) as IPostgreSqlContinuedCreate<TEntity>;
    #endregion

    #region WithBulk
    public override IPostgreSqlBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
        => base.WithBulk(insertObjs, bulkCount) as IPostgreSqlBulkContinuedCreate<TEntity>;
    #endregion

    #region WithBulkCopy
    public IPostgreSqlCreated<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null)
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
        if (isEmpty) throw new Exception("批量更新，updateObjs参数至少要有一条数据");

        this.DialectVisitor.WithBulkCopy(insertObjs, timeoutSeconds);
        return this.OrmProvider.NewCreated<TEntity>(this.DbContext, this.Visitor) as IPostgreSqlCreated<TEntity>;
    }
    #endregion
}