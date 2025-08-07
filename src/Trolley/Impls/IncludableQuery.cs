using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class IncludableQuery<T, TMember> : Query<T>, IIncludableQuery<T, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, TMember> : Query<T1, T2>, IIncludableQuery<T1, T2, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, TMember> : Query<T1, T2, T3>, IIncludableQuery<T1, T2, T3, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, TMember> : Query<T1, T2, T3, T4>, IIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, TMember> : Query<T1, T2, T3, T4, T5>, IIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : Query<T1, T2, T3, T4, T5, T6>, IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : Query<T1, T2, T3, T4, T5, T6, T7>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, beginFieldValue, endFieldValue);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, beginField2Value, endField2Value);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, field1Value, field2Value, beginField3Value, endField3Value);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation>(this.DbContext, this.Visitor);
    }
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion
}