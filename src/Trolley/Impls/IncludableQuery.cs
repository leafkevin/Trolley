using System;

namespace Trolley;

public class IncludableQuery<T, TMember> : Query<T>, IIncludableQuery<T, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public IIncludableQuery<T, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, TMember> : Query<T1, T2>, IIncludableQuery<T1, T2, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, TMember> : Query<T1, T2, T3>, IIncludableQuery<T1, T2, T3, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, TMember> : Query<T1, T2, T3, T4>, IIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, TMember> : Query<T1, T2, T3, T4, T5>, IIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : Query<T1, T2, T3, T4, T5, T6>, IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : Query<T1, T2, T3, T4, T5, T6, T7>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public IncludableQuery(DbContext dbContext, IQueryVisitor visitor, TableSegment tableSegment)
        : base(dbContext, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion
}