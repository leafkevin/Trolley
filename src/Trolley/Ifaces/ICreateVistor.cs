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
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }

    string BuildCommand(IDbCommand command, bool isReturnIdentity);
    MultipleCommand CreateMultipleCommand();
    IQueryVisitor CreateQueryVisitor();
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    string BuildSql();
    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(IEnumerable insertObjs, int bulkCount);
    (IEnumerable, int, Action<StringBuilder>, Action<StringBuilder, object, string>) BuildWithBulk(IDbCommand command);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
    DataTable ToDataTable(Type entityType, IEnumerable entities, EntityMap fromMapper, string tableName = null);
    List<(MemberInfo MemberInfo, MemberMap RefMemberMapper)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper);
}
