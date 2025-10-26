using System;
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

    string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields);

    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTableByRange(bool isIncludeMany, object[] fieldValues);
    void UseTableMap(bool isIncludeMany, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, params object[] fieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void WhereWith(object wherKeys);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
    void Or(Expression whereExpr);

    string GetTableName(TableSegment tableSegment);
    string BuildTableShardingsSql();
}