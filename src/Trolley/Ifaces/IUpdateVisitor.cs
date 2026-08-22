using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IUpdateVisitor : ICommandVisitor, IDisposable
{
    DbContext DbContext { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider EntityMapProvider { get; }
    List<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    bool HasWhere { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> ShardingTables { get; set; }


    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>, Action<IDataParameterCollection,
         StringBuilder, DbContext, string, object, string>, List<ReaderField>) BuildSetBulk(ITheaCommand command);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableMap(TableShardingUsageMode usageMode, bool isIncludeMany, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseTable(TableShardingUsageMode usageMode, Func<object, string> tableNameGetter);
    void UseTableSchema(bool isIncludeMany, string tableSchema);
    void WithTableAliasTrailing(bool isIncludeMany, string rawSql);

    void Join(string joinType, Type entityType, Expression joinOn);
    void SetExpr(Expression fieldsAssignment);
    void SetObject(object updateObj);
    void SetField(string fieldName, object fieldValue);
    void SetField(Expression fieldSelector, object fieldValue);
    void SetFrom(Expression fieldsAssignment);
    void SetFrom(Expression fieldSelector, Expression valueSelector);
    void IgnoreFields(params string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(params string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    void SetBulk(IEnumerable updateObjs, int bulkCount);

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

    DataTable ToDataTable(string tableName, IEnumerable entities, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters);
    (List<MemberMap>, List<Func<object, object>>) GetRefMemberMappers(Type parameterType, EntityMap entityMapper, object parameterSample, bool isUpdate = false);

    bool IsMemberVisit(Expression expr);
}