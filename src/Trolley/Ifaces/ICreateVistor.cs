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

    string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields);
    IQueryVisitor CreateQueryVisitor(char? tableAsStart = null);
    void Initialize(Type entityType);
    string BuildSql(out List<SqlFieldSegment> readerFields);

    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTable<TInsertObj>(Func<string, TInsertObj, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, params object[] fieldValues);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, int>, string, List<SqlFieldSegment>) BuildWithBulk();
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(Type insertObjType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type insertObjType, EntityMap refEntityMapper, bool isUpdate = false);
    Dictionary<string, List<object>> SplitShardingParameters(Type insertObjType, TableShardingInfo tableShardingInfo, IEnumerable insertObjs, object insertObjSample);
}