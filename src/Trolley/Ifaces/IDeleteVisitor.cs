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
    List<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    bool HasWhere { get; }
    List<TableSegment> ShardingTables { get; }


    void UseTable(TableUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableBy(TableUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTableByRange(TableUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);
    void WithTableAliasTrailing(bool isIncludeMany, string rawSql);

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