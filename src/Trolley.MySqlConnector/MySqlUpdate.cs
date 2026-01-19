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
    public new IMySqlUpdate<TEntity> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IMySqlUpdate<TEntity>;
    public new IMySqlUpdate<TEntity> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IMySqlUpdate<TEntity>;
    public new IMySqlUpdate<TEntity> UseTable(Func<object, string> tableNameGetter)
        => base.UseTable(tableNameGetter) as IMySqlUpdate<TEntity>;
    public new IMySqlUpdate<TEntity> UseTableByRange(params object[] fieldValues)
        => base.UseTableByRange(fieldValues) as IMySqlUpdate<TEntity>;
    #endregion

    #region UseTableSchema
    public new IMySqlUpdate<TEntity> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IMySqlUpdate<TEntity>;
    #endregion

    #region Set
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(TFields setObj)
        => this.Set(true, setObj);
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, TFields setObj)
        => base.Set(condition, setObj) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public new IMySqlContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => base.Set(condition, fieldSelector, fieldValue) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public new IMySqlContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
        => base.Set(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region SetFrom
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
        => base.SetFrom(condition, fieldSelector, valueSelector) as IMySqlContinuedUpdate<TEntity>;
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public new IMySqlContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => base.SetFrom(condition, fieldsAssignment) as IMySqlContinuedUpdate<TEntity>;
    #endregion

    #region SetBulk
    public new IMySqlBulkContinuedUpdate<TEntity> SetBulk<TUpdateObj>(IEnumerable<TUpdateObj> updateObjs, int bulkCount = 500)
        => base.SetBulk(updateObjs, bulkCount) as IMySqlBulkContinuedUpdate<TEntity>;
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
    public IMySqlBulkCopyContinuedUpdate<TEntity> SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null)
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
        return this.OrmProvider.NewBulkCopyContinuedUpdate<TEntity>(this.DbContext, this.Visitor) as IMySqlBulkCopyContinuedUpdate<TEntity>;
    }
    #endregion
}