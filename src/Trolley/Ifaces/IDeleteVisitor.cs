using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IDeleteVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    List<TableSegment> Tables { get; }
    ITableShardingProvider ShardingProvider { get; }
    bool HasWhere { get; }
    List<TableSegment> ShardingTables { get; }

    string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void AndBy(object whereObj);
    void AndById(object whereKey);
    void AndByIds(IEnumerable whereKeys); 
    void And(Expression whereExpr);
    void OrBy(object whereObj);
    void OrById(object whereKey);
    void OrByIds(IEnumerable whereKeys);
    void Or(Expression whereExpr);

    string GetTableName(TableSegment tableSegment);
    string BuildTableShardingsSql();
}