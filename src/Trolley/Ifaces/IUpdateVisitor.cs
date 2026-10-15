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
    ReusableList<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    bool HasWhere { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> ShardingTables { get; set; }


    (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>, Action<IDataParameterCollection,
         StringBuilder, DbContext, string, object, string>, ReusableList<ReaderField>) BuildSetBulk(ITheaCommand command);

    void UseTable(TableSegment tableSegment, params string[] tableNames);
    void UseTable(TableSegment tableSegment, Func<object, string> tableNameGetter);
    void UseTableBy(TableUsageMode usageMode, TableSegment tableSegment, params object[] fieldValues);
    void UseTableByRange(TableUsageMode usageMode, TableSegment tableSegment, object[] fieldValues);
    void UseTableMap(TableSegment tableSegment, Func<string, string, string, string> tableNameGetter);
    void UseTableSchema(TableSegment tableSegment, string tableSchema);
    void WithTableAliasTrailing(TableSegment tableSegment, string rawSql);

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