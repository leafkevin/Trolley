using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MultiIncludableQuery<T, TMember> : MultiQuery<T>, IMultiIncludableQuery<T, TMember>
{
    #region Properties
    protected TableSegment TableSegment { get; private set; }
    #endregion

    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor, TableSegment tableSegment)
        : base(multiQuery, visitor)
    {
        this.TableSegment = tableSegment;
    }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion

    #region WithTableAliasTrailing
    public new IMultiIncludableQuery<T, TMember> WithTableAliasTrailing(string rawSql)
    {
        this.Visitor.WithTableAliasTrailing(this.TableSegment, rawSql);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, TMember> : MultiQuery<T1, T2>, IMultiIncludableQuery<T1, T2, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, TMember> : MultiQuery<T1, T2, T3>, IMultiIncludableQuery<T1, T2, T3, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, TMember> : MultiQuery<T1, T2, T3, T4>, IMultiIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, TMember> : MultiQuery<T1, T2, T3, T4, T5>, IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Sharding
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
    {
        this.Visitor.UseTable(this.TableSegment, tableNames);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap(Func<string, string, string, string> tableNameGetter)
    {
        this.Visitor.UseTableMap(this.TableSegment, tableNameGetter);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues)
    {
        this.Visitor.UseTableBy(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(params object[] fieldValues)
    {
        this.Visitor.UseTableByRange(TableUsageMode.ReadOnly, this.TableSegment, fieldValues);
        return this;
    }
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseUnionShardingTable()
    {
        this.Visitor.UseUnionShardingTable();
        return this;
    }
    #endregion

    #region UseTableSchema
    public new IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
    {
        this.Visitor.UseTableSchema(this.TableSegment, tableSchema);
        return this;
    }
    #endregion
}
public class MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Constructor
    public MultiIncludableQuery(IMultipleQuery multiQuery, IQueryVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion
}