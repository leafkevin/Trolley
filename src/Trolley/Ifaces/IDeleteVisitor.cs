using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public interface IDeleteVisitor : ICommandVisitor, IDisposable
{
    DbContext DbContext { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider EntityMapProvider { get; }
    ReusableList<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    bool HasWhere { get; }
    List<TableSegment> ShardingTables { get; }


    void UseTable(TableSegment tableSegment, params string[] tableNames);
    void UseTable(TableSegment tableSegment, Func<object, string> tableNameGetter);
    void UseTableBy(TableUsageMode usageMode, TableSegment tableSegment, params object[] fieldValues);
    void UseTableByRange(TableUsageMode usageMode, TableSegment tableSegment, object[] fieldValues);
    void UseTableSchema(TableSegment tableSegment, string tableSchema);
    void WithTableAliasTrailing(TableSegment tableSegment, string rawSql);

    void AndBy(object whereObj);
    void AndById(object whereKey);
    void AndByIds(IEnumerable whereKeys);
    void And(Expression whereExpr);
    void OrBy(object whereObj);
    void OrById(object whereKey);
    void OrByIds(IEnumerable whereKeys);
    void Or(Expression whereExpr);

    void WithLeadingSql(string rawSql);
    void WithTrailingSql(string rawSql);
}