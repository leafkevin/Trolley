using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public interface IUpdateVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    IShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(IDbCommand command);
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    string BuildSql();

    #region Sharding
    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsAssignment);
    IUpdateVisitor SetWith(object updateObj);
    IUpdateVisitor SetField(Expression fieldSelector, object fieldValue);
    IUpdateVisitor SetFrom(Expression fieldsAssignment);
    IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector);
    IUpdateVisitor IgnoreFields(params string[] fieldNames);
    IUpdateVisitor IgnoreFields(Expression fieldsSelector);
    IUpdateVisitor OnlyFields(params string[] fieldNames);
    IUpdateVisitor OnlyFields(Expression fieldsSelector);
    IUpdateVisitor SetBulk(IEnumerable updateObjs, int bulkCount);
    (IEnumerable, int, Action<StringBuilder, object, string>, Action<IDataParameterCollection>) BuildSetBulk(IDbCommand command);
    IUpdateVisitor WhereWith(object whereObj);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
    DataTable ToDataTable(Type entityType, IEnumerable entities, EntityMap fromMapper, string tableName = null);
    List<(MemberInfo MemberInfo, MemberMap RefMemberMapper)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper);
}
