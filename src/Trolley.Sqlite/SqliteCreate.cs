using System.Collections;

namespace Trolley.Sqlite;

public class SqliteCreate<TEntity> : Create<TEntity>, ISqliteCreate<TEntity>
{
    #region Properties
    public SqliteCreateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqliteCreate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as SqliteCreateVisitor;
    }
    #endregion

    #region Sharding
    public new ISqliteCreate<TEntity> UseTable(string tableName)
        => base.UseTable(tableName) as ISqliteCreate<TEntity>;
    public new ISqliteCreate<TEntity> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as ISqliteCreate<TEntity>;
    #endregion

    #region OrIgnore/OrReplace/OrAbort/OrFail/OrRollback
    public ISqliteCreate<TEntity> OrIgnore()
    {
        this.DialectVisitor.OrExpression(" OR IGNORE");
        return this;
    }
    public ISqliteCreate<TEntity> OrReplace()
    {
        this.DialectVisitor.OrExpression(" OR REPLACE");
        return this;
    }
    public ISqliteCreate<TEntity> OrAbort()
    {
        this.DialectVisitor.OrExpression(" OR ABORT");
        return this;
    }
    public ISqliteCreate<TEntity> OrFail()
    {
        this.DialectVisitor.OrExpression(" OR FAIL");
        return this;
    }
    public ISqliteCreate<TEntity> OrRollback()
    {
        this.DialectVisitor.OrExpression(" OR ROLLBACK");
        return this;
    }
    #endregion

    #region WithBy
    public new ISqliteContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj)
        => base.WithBy(insertObj) as ISqliteContinuedCreate<TEntity>;
    #endregion

    #region WithBulk
    public new ISqliteBulkContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount)
        => base.WithBulk(insertObjs, bulkCount) as ISqliteBulkContinuedCreate<TEntity>;
    #endregion

    #region From
    public new ISqliteFromCommand<T> From<T>()
        => base.From<T>() as ISqliteFromCommand<T>;
    public new ISqliteFromCommand<T1, T2> From<T1, T2>()
        => base.From<T1, T2>() as ISqliteFromCommand<T1, T2>;
    public new ISqliteFromCommand<T1, T2, T3> From<T1, T2, T3>()
       => base.From<T1, T2, T3>() as ISqliteFromCommand<T1, T2, T3>;
    public new ISqliteFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
        => base.From<T1, T2, T3, T4>() as ISqliteFromCommand<T1, T2, T3, T4>;
    public new ISqliteFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
        => base.From<T1, T2, T3, T4, T5>() as ISqliteFromCommand<T1, T2, T3, T4, T5>;
    public new ISqliteFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
        => base.From<T1, T2, T3, T4, T5, T6>() as ISqliteFromCommand<T1, T2, T3, T4, T5, T6>;
    public new ISqliteFromCommand<T> From<T>(IQuery<T> subQuery)
        => base.From(subQuery) as ISqliteFromCommand<T>;
    #endregion
}