using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public class SqliteUpdate<TEntity> : Update<TEntity>, ISqliteUpdate<TEntity>
{
    #region Properties
    public SqliteUpdateVisitor DialectVisitor { get; private set; }
    #endregion

    #region Constructor
    public SqliteUpdate(DbContext dbContext) : base(dbContext)
    {
        this.DialectVisitor = this.Visitor as SqliteUpdateVisitor;
    }
    #endregion

    #region Sharding
    public new ISqliteUpdate<TEntity> UseTable(params string[] tableNames)
		=> base.UseTable(tableNames) as ISqliteUpdate<TEntity>;
    public new ISqliteUpdate<TEntity> UseTable(Func<string, bool> tableNamePredicate)
		=> base.UseTable(tableNamePredicate) as ISqliteUpdate<TEntity>;
    public new ISqliteUpdate<TEntity> UseTableBy(object field1Value, object field2Value = null)
    	=> base.UseTableBy(field1Value, field2Value) as ISqliteUpdate<TEntity>;
    public new ISqliteUpdate<TEntity> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as ISqliteUpdate<TEntity>;

    public new ISqliteUpdate<TEntity> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as ISqliteUpdate<TEntity>;
    #endregion

    #region Set
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => this.Set(true, setObj);
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
        => base.Set(condition, setObj) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new ISqliteContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new ISqliteContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom    
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as ISqliteContinuedUpdate<TEntity>;
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public new ISqliteContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as ISqliteContinuedUpdate<TEntity>;
    #endregion

    #region SetBulk
    public new ISqliteContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
        => base.SetBulk(updateObjs, bulkCount) as ISqliteContinuedUpdate<TEntity>;
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
    public ISqliteUpdated<TEntity> SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null)
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
        return this.OrmProvider.NewUpdated<TEntity>(this.DbContext, this.Visitor) as ISqliteUpdated<TEntity>;
    }
    #endregion
}
