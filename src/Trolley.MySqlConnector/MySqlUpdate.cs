using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public class MySqlUpdate<TEntity> : Update<TEntity>, IMySqlUpdate<TEntity>
{
    #region Properties
    public MySqlUpdateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public MySqlUpdate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as MySqlUpdateVisitor;
    }
    #endregion

    #region Sharding
    public override IMySqlUpdate<TEntity> UseTable(params string[] tableNames)
    {
        base.UseTable(tableNames);
        return this;
    }
    public override IMySqlUpdate<TEntity> UseTable(Func<string, bool> tableNamePredicate)
    {
        base.UseTable(tableNamePredicate);
        return this;
    }
    public override IMySqlUpdate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    {
        base.UseTableBy(field1Value, field2Value);
        return this;
    }
    public override IMySqlUpdate<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        base.UseTableByRange(beginFieldValue, endFieldValue);
        return this;
    }
    public override IMySqlUpdate<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion


    #region Set
    public override IMySqlContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => base.Set(setObj) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
        => base.Set(condition, setObj) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(fieldSelector, fieldValue) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom    
    public override IMySqlContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(fieldSelector, valueSelector) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    public override IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region SetBulk
    public override IMySqlContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
        => base.SetBulk(updateObjs, bulkCount) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region Join
    public IUpdateJoin<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("INNER JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    public IUpdateJoin<TEntity, T> LeftJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.Visitor.Join("LEFT JOIN", typeof(T), joinOn);
        return this.OrmProvider.NewUpdateJoin<TEntity, T>(this.DbContext, this.Visitor);
    }
    #endregion

    #region WithBulkCopy
    public IMySqlUpdated<TEntity> SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null)
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
        this.DialectVisitor.WithBulkCopy(updateObjs, timeoutSeconds);
        return this.OrmProvider.NewUpdated<TEntity>(this.DbContext, this.Visitor) as IMySqlUpdated<TEntity>;
    }
    #endregion
}
