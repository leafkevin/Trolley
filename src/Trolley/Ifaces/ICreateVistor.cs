using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public interface ICreateVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    IShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }

    string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<ReaderField> readerFields);
    MultipleCommand CreateMultipleCommand();
    IQueryVisitor CreateQueryVisitor();
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    string BuildSql(out List<ReaderField> readerFields);

    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTable(Type entityType, Type masterEntityType, Func<string, string, string, string, string> tableNameGetter);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);

    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<ReaderField>) BuildWithBulk(IDbCommand command);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(Type insertObjType, IEnumerable entities, List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> memberMappers, string tableName = null);
    List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> GetRefMemberMappers(Type insertObjType, EntityMap refEntityMapper, bool isUpdate = false);
}
