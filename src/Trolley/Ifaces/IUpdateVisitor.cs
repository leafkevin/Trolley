using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IUpdateVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    bool HasWhere { get; }
    ITableShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }
    List<TableSegment> ShardingTables { get; set; }

    string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableMap(TableShardingUsageMode usageMode, bool isIncludeMany, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTable<TUpdateObj>(TableShardingUsageMode usageMode, Func<string, TUpdateObj, string> tableNameGetter);
    void UseTableByOthers(TableShardingUsageMode usageMode, params object[] otherFieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void Join(string joinType, Type entityType, Expression joinOn);
    void Set(Expression fieldsAssignment);
    void SetWith(object updateObj);
    void SetField(Expression fieldSelector, object fieldValue);
    void SetFrom(Expression fieldsAssignment);
    void SetFrom(Expression fieldSelector, Expression valueSelector);
    void IgnoreFields(params string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(params string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    void SetBulk(IEnumerable updateObjs, int bulkCount);
    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, string, object, int>, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command);

    void WhereWith(object whereObj);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
    void Or(Expression whereExpr);
    DataTable ToDataTable(Type updateObjType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper, bool isUpdate = false);
    string BuildTableShardingsSql();
    string GetTableName(TableSegment tableSegment);
    bool IsMemberVisit(Expression expr);
}