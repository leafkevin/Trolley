using System;
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

    #region Sharding
    public new IMySqlCreate<TEntity> UseTable(string tableName)
        => base.UseTable(tableName) as IMySqlCreate<TEntity>;
    public new IMySqlCreate<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlCreate<TEntity>;
    public new IMySqlCreate<TEntity> UseTableBy<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter)
        => base.UseTableBy(tableNameGetter) as IMySqlCreate<TEntity>;
    #endregion

    #region UseTableSchema
    public new IMySqlCreate<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableBy(tableSchema) as IMySqlCreate<TEntity>;
    #endregion

    #region IgnoreInto
    public virtual IMySqlCreate<TEntity> IgnoreInto()
    {
        this.DialectVisitor.IsUseIgnoreInto = true;
        return this;
    }
    #endregion

    #region WithBy
    public new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => base.WithBy(insertObj) as IMySqlContinuedCreate<TEntity>;
    #endregion

    #region WithBulk
    public new IMySqlBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
        => base.WithBulk(insertObjs, bulkCount) as IMySqlBulkContinuedCreate<TEntity>;
    #endregion

    #region WithBulkCopy
    public ICreated<TEntity> WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds = null)
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
        return this.OrmProvider.NewCreated<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public new IMySqlFromCommand<T> From<T>()
        => base.From<T>() as IMySqlFromCommand<T>;
    public new IMySqlFromCommand<T1, T2> From<T1, T2>()
        => base.From<T1, T2>() as IMySqlFromCommand<T1, T2>;
    public new IMySqlFromCommand<T1, T2, T3> From<T1, T2, T3>()
       => base.From<T1, T2, T3>() as IMySqlFromCommand<T1, T2, T3>;
    public new IMySqlFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
        => base.From<T1, T2, T3, T4>() as IMySqlFromCommand<T1, T2, T3, T4>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
        => base.From<T1, T2, T3, T4, T5>() as IMySqlFromCommand<T1, T2, T3, T4, T5>;
    public new IMySqlFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
        => base.From<T1, T2, T3, T4, T5, T6>() as IMySqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IMySqlFromCommand<T> From<T>(IQuery<T> subQuery)
        => base.From(subQuery) as IMySqlFromCommand<T>;
    #endregion
}