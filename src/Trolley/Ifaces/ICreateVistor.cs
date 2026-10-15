using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor : ICommandVisitor, IDisposable
{
    DbContext DbContext { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider EntityMapProvider { get; }
    ReusableList<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    ActionMode ActionMode { get; set; }
    bool IsReturnIdentity { get; set; }

    ReusableList<IQuery> SharedQueryObjs { get; set; }
    List<TableSegment> ShardingTables { get; set; }
    Dictionary<string, TableSegment> RefTableAliases { get; set; }
    ICteQuery CteQueryObj { get; set; }
    string FromSql { get; set; }
    bool IsRecursive { get; set; }


    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, ReusableList<ReaderField>) BuildWithBulk(ITheaCommand command);

    void UseTable(TableSegment tableSegment, params string[] tableNames);
    void UseTable(TableSegment tableSegment, Func<object, string> tableNameGetter);
    void UseTableBy(TableUsageMode usageMode, TableSegment tableSegment, params object[] fieldValues);  
    void UseTableSchema(TableSegment tableSegment, string tableSchema);
    void WithTableAliasTrailing(TableSegment tableSegment, string rawSql);

    void WithBy(object insertObj);
    void WithByField(string fieldName, object fieldValue);
    void WithByFieldExpr(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);

    void WithLeadingSql(string rawSql);
    void WithTrailingSql(string rawSql);

    DataTable ToDataTable(string tableName, IEnumerable entities, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters);
    (List<MemberMap>, List<Func<object, object>>) GetRefMemberMappers(Type parameterType, EntityMap entityMapper, object parameterSample, bool isUpdate = false);
    Dictionary<string, List<object>> SplitShardingParameters(TableShardingInfo tableShardingInfo, TableUsageMode usageMode, Type paramterType, IEnumerable parameters, object parameterSample, IDictionary<string, object> shardingValues);
}