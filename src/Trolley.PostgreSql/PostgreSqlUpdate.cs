using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdate<TEntity> : Update<TEntity>, IPostgreSqlUpdate<TEntity>
{
    #region Properties
    public PostgreSqlUpdateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public PostgreSqlUpdate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlUpdateVisitor;
    }
    #endregion

    #region Sharding
    public override IPostgreSqlUpdate<TEntity> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlUpdate<TEntity>;
    public override IPostgreSqlUpdate<TEntity> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlUpdate<TEntity>;
    public override IPostgreSqlUpdate<TEntity> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlUpdate<TEntity>;
    public override IPostgreSqlUpdate<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlUpdate<TEntity>;
    public override IPostgreSqlUpdate<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlUpdate<TEntity>;
    #endregion

    #region Set
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => this.Set(true, setObj);
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
        => base.Set(condition, setObj) as IPostgreSqlContinuedUpdate<TEntity>;
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IPostgreSqlContinuedUpdate<TEntity>;
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public override IPostgreSqlContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as IPostgreSqlContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom    
    public override IPostgreSqlContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public override IPostgreSqlContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as IPostgreSqlContinuedUpdate<TEntity>;
    public override IPostgreSqlContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public override IPostgreSqlContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as IPostgreSqlContinuedUpdate<TEntity>;
    #endregion

    #region SetBulk
    public override IPostgreSqlContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
        => base.SetBulk(updateObjs, bulkCount) as IPostgreSqlContinuedUpdate<TEntity>;
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        this.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        if (joinOn == null) throw new ArgumentNullException(nameof(joinOn));
        this.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulkCopy
    public IPostgreSqlUpdated<TEntity> SetBulkCopy(IEnumerable updateObjs)
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
        this.DialectVisitor.WithBulkCopy(updateObjs);
        return this.OrmProvider.NewUpdated<TEntity>(this.DbContext, this.Visitor) as IPostgreSqlUpdated<TEntity>;
    }
    #endregion
}
