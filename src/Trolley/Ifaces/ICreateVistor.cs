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

    string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields);
    MultipleCommand CreateMultipleCommand();
    IQueryVisitor CreateQueryVisitor();
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    string BuildSql(out List<SqlFieldSegment> readerFields);

    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTableBy(bool isIncludeMany, object field1Value, object field2Value = null);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void WithBy(object insertObj, ActionMode? actionMode = null);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<SqlFieldSegment>) BuildWithBulk(IDbCommand command);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(Type insertObjType, IEnumerable entities, List<(MemberMap, Func<object, object>)> memberMappers, string tableName = null);
    List<(MemberMap, Func<object, object>)> GetRefMemberMappers(Type insertObjType, EntityMap refEntityMapper, bool isUpdate = false);
}
