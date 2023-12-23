using System.Collections;

namespace Trolley.MySqlConnector;

public class MySqlCreate<TEntity> : Create<TEntity>, IMySqlCreate<TEntity>
{
    #region Properties
    public MySqlCreateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
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

    #region From
    public IFromCommand<T> From<T>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2> From<T1, T2>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T1, T2>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3> From<T1, T2, T3>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T1, T2, T3>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    public IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        //TODO:需要测试dbParameters是否有值
        var queryVisitor = this.DialectVisitor.CreateQueryVisitor();
        return this.OrmProvider.NewFromCommand<T1, T2, T3, T4, T5, T6>(typeof(TEntity), this.DbContext, queryVisitor);
    }
    #endregion
}
