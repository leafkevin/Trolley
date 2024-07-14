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
    bool HasWhere { get; }
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    IShardingProvider ShardingProvider { get; }
    ActionMode ActionMode { get; set; }
    List<TableSegment> Tables { get; }
    bool IsNeedFetchShardingTables { get; }
    List<TableSegment> ShardingTables { get; set; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(DbContext dbContext, IDbCommand command);
    void BuildMultiCommand(DbContext dbContext, IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);

    #region Sharding
    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTable(Type entityType, Type masterEntityType, Func<string, string, string, string, string> tableNameGetter);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

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
    (IEnumerable, int, string, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>,
        Action<StringBuilder, string>, Action<StringBuilder, IOrmProvider, object, string>) BuildWithBulk(IDbCommand command);
    void WhereWith(object whereObj);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
    DataTable ToDataTable(Type entityType, IEnumerable entities, EntityMap fromMapper, string tableName = null);
    List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> GetRefMemberMappers(Type entityType, EntityMap refEntityMapper, bool isUpdate = false);
    string BuildShardingTablesSql(string tableSchema);
    void SetShardingTables(List<string> shardingTables);
    string GetTableName(TableSegment tableSegment);
}
