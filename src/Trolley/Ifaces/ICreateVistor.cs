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

    string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields);
    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command);

    IQueryVisitor CreateQueryVisitor(char? tableAsStart = null);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTable(TableShardingUsageMode usageMode, Func<string, object, string> tableNameGetter);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(string tableName, Type parameterType, IEnumerable entities, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters);
    (List<MemberMap>, List<Func<object, object>>) GetRefMemberMappers(Type parameterType, EntityMap entityMapper, object parameterSample, bool isUpdate = false);
    Dictionary<string, List<object>> SplitShardingParameters(Type paramterType, IEnumerable parameters, object parameterSample);
}