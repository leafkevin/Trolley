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
    public new IPostgreSqlCreate<TEntity> UseTable(string tableName)
        => base.UseTable(tableName) as IPostgreSqlCreate<TEntity>;
    public new IPostgreSqlCreate<TEntity> UseTable<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter)
        => base.UseTable(tableNameGetter) as IPostgreSqlCreate<TEntity>;
    public new IPostgreSqlCreate<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlCreate<TEntity>;    
    #endregion

    #region UseTableSchema
    public new IPostgreSqlCreate<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlCreate<TEntity>;
    #endregion

    #region WithBy
    public new IPostgreSqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => base.WithBy(insertObj) as IPostgreSqlContinuedCreate<TEntity>;
    #endregion

    #region WithBulk
    public new IPostgreSqlBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
        => base.WithBulk(insertObjs, bulkCount) as IPostgreSqlBulkContinuedCreate<TEntity>;
    #endregion

    #region WithBulkCopy
    public ICreated<TEntity> WithBulkCopy(IEnumerable insertObjs)
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

        this.DialectVisitor.WithBulkCopy(insertObjs);
        return this.OrmProvider.NewCreated<TEntity>(this.DbContext, this.Visitor);
    }
    #endregion

    #region From
    public new IPostgreSqlFromCommand<T> From<T>()
        => base.From<T>() as IPostgreSqlFromCommand<T>;
    public new IPostgreSqlFromCommand<T1, T2> From<T1, T2>()
        => base.From<T1, T2>() as IPostgreSqlFromCommand<T1, T2>;
    public new IPostgreSqlFromCommand<T1, T2, T3> From<T1, T2, T3>()
       => base.From<T1, T2, T3>() as IPostgreSqlFromCommand<T1, T2, T3>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
        => base.From<T1, T2, T3, T4>() as IPostgreSqlFromCommand<T1, T2, T3, T4>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
        => base.From<T1, T2, T3, T4, T5>() as IPostgreSqlFromCommand<T1, T2, T3, T4, T5>;
    public new IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
        => base.From<T1, T2, T3, T4, T5, T6>() as IPostgreSqlFromCommand<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlFromCommand<T> From<T>(IQuery<T> subQuery)
        => base.From(subQuery) as IPostgreSqlFromCommand<T>;
    #endregion
}