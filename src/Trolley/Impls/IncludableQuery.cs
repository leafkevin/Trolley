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
    public override IIncludableQuery<T, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>(this.DbContext, this.Visitor);
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
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNames);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(Func<string, bool> tableNamePredicate)
    {
        this.Visitor.UseTable(this.IsIncludeMany, tableNamePredicate);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable<TMasterSharding>(Func<string, string, string, string, string> tableNameGetter)
    {
        var masterEntityType = typeof(TMasterSharding);
        this.Visitor.UseTable(this.IsIncludeMany, masterEntityType, tableNameGetter);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(object field1Value, object field2Value = null)
    {
        this.Visitor.UseTableBy(this.IsIncludeMany, field1Value, field2Value);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, beginFieldValue, endFieldValue);
        return this;
    }
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
    {
        this.Visitor.UseTableByRange(this.IsIncludeMany, fieldValue1, fieldValue2, fieldValue3);
        return this;
    }
    #endregion

    #region UseTableSchema
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
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
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
    {
        this.Visitor.ThenInclude(member, filter);
        return this.OrmProvider.NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment>(this.DbContext, this.Visitor);
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
