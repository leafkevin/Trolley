using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MultiIncludableQuery<T, TMember> : MultiQuery<T>, IMultiIncludableQuery<T, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, TMember> : MultiQuery<T1, T2>, IMultiIncludableQuery<T1, T2, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, TMember> : MultiQuery<T1, T2, T3>, IMultiIncludableQuery<T1, T2, T3, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, TMember> : MultiQuery<T1, T2, T3, T4>, IMultiIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, TMember> : MultiQuery<T1, T2, T3, T4, T5>, IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTableMap(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(false, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(false, [beginFieldValue, endFieldValue]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, beginField2Value, endField2Value]);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
    {
        this.Visitor.UseTableByRange(false, [field1Value, field2Value, beginField3Value, endField3Value]);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.IsIncludeMany, tableSchema);
        return this;
    }
    #endregion

    #region ThenInclude/ThenIncludeMany
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
    {
        var isIncludeMany = this.Visitor.ThenInclude(member);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation>(this.MultipleQuery, this.Visitor, isIncludeMany);
    }
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>(this.MultipleQuery, this.Visitor, true);
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Properties
    public bool IsIncludeMany { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, bool isIncludeMany)
        : base(multiQuery, visitor)
    {
        this.IsIncludeMany = isIncludeMany;
    }
    #endregion
}