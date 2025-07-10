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
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    ITableShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }
    bool IsNeedFetchShardingTables { get; }
    List<TableSegment> ShardingTables { get; set; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(DbContext dbContext, ITheaCommand command, out List<SqlFieldSegment> readerFields);
    void BuildMultiCommand(DbContext dbContext, ITheaCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);

    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTable(bool isIncludeMany, Func<string, bool> tableNamePredicate);
    void UseTable<TUpdateObj>(Func<string, TUpdateObj, string> tableNameGetter);
    void UseTableMap(bool isIncludeMany, Type masterEntityType, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, params object[] fieldValues);    
    void UseTableByRange(bool isIncludeMany, object beginFieldValue, object endFieldValue);
    void UseTableByRange(bool isIncludeMany, object field1Value, object beginField2Value, object endField2Value);
    void UseTableByRange(bool isIncludeMany, object field1Value, object field2Value, object beginField3Value, object endField3Value);
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
    (IEnumerable, int, string, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>,
        Action<StringBuilder, DbContext, string, object, string>, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command);
    void WhereWith(object whereObj);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
    void Or(Expression whereExpr);
    DataTable ToDataTable(Type updateObjType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper, bool isUpdate = false);
    string BuildTableShardingsSql();
    bool SetShardingTables(List<string> shardingTables);
    string GetTableName(TableSegment tableSegment);
    bool IsMemberVisit(Expression expr);
}