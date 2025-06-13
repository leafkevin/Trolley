using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IDeleteVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    List<TableSegment> Tables { get; }
    ITableShardingProvider ShardingProvider { get; }
    bool HasWhere { get; }
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    bool IsNeedFetchShardingTables { get; }
    List<TableSegment> ShardingTables { get; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields);
    void BuildMultiCommand(ITheaCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);

    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTable(bool isIncludeMany, Func<string, bool> tableNamePredicate);
    void UseTableMap(bool isIncludeMany, Type masterEntityType, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, params object[] fieldValues);
    void UseTableByRange(bool isIncludeMany, object beginFieldValue, object endFieldValue);
    void UseTableByRange(bool isIncludeMany, object fieldValue1, object beginField2Value, object endField2Value);
    void UseTableByRange(bool isIncludeMany, object fieldValue1, object fieldValue2, object beginField3Value, object endField3Value);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    IDeleteVisitor WhereWith(object wherKeys);
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);

    string GetTableName(TableSegment tableSegment);
    string BuildTableShardingsSql();
    bool SetShardingTables(List<string> shardingTables);
}