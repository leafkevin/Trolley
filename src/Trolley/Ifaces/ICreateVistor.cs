using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    ITableShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }
    bool IsReturnIdentity { get; set; }

    string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields);
    string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields);
    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command);

    IQueryVisitor CreateQueryVisitor(char? tableAsStart = null);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTable<TParameter>(TableShardingUsageMode usageMode, Func<string, TParameter, string> tableNameGetter);
    void UseTableByOthers(TableShardingUsageMode usageMode, params object[] otherFieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(Type insertObjType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type insertObjType, EntityMap refEntityMapper, bool isUpdate = false);
    Dictionary<string, List<object>> SplitShardingParameters(Type paramterType, IEnumerable parameters, Dictionary<string, object> shardingValues);
}