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
    IShardingProvider ShardingProvider { get; }
    bool HasWhere { get; }
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    bool IsNeedFetchShardingTables { get; }
    List<TableSegment> ShardingTables { get; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(DbContext dbContext, IDbCommand command);
    void BuildMultiCommand(DbContext dbContext, IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);

    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTable(Type entityType, Type masterEntityType, Func<string, string, string, string, string> tableNameGetter);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);

    IDeleteVisitor WhereWith(object wherKeys);
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);

    string GetTableName(TableSegment tableSegment);
    string BuildShardingTablesSql(string tableSchema);
    void SetShardingTables(List<string> shardingTables);
}