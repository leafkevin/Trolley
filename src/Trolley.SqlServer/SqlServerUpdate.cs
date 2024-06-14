using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;
/// <summary>
/// UPDATE sales.commissions
/// SET sales.commissions.commission =  c.base_amount* COALESCE(t.percentage,0.1)
/// FROM sales.commissions c
/// LEFT JOIN sales.targets t ON c.target_id = t.target_id;
/// 
/// USE AdventureWorks2022;
/// GO
/// UPDATE p
/// SET ListPrice = ListPrice * 2
/// FROM Production.Product AS p
///  INNER JOIN Purchasing.ProductVendor AS pv
///      ON p.ProductID = pv.ProductID AND p.ProductModelID is not null  
/// WHERE ...
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class SqlServerUpdate<TEntity> : Update<TEntity>, ISqlServerUpdate<TEntity>
{
    #region Constructor
    public SqlServerUpdate(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region InnerJoin
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion

    #region LeftJoin
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion
}
