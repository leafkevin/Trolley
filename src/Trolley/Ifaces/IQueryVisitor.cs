using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQueryVisitor : ICloneable, IDisposable
{
    string DbKey { get; }
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    string WhereSql { get; }
    List<TableSegment> Tables { get; set; }
    List<TableSegment> IncludeTables { get; set; }
    Dictionary<string, TableSegment> TableAliases { get; }
    /// <summary>
    /// 在解析子查询中，会用到父查询中的所有表，父查询中所有表别名引用
    /// </summary>
    Dictionary<string, TableSegment> RefTableAliases { get; set; }
    bool IsCteTable { get; set; }
    /// <summary>
    /// 在SQL查询中，引用到子查询或是CTE表对象，防止重复添加参数，同时也为了解析CTE表引用SQL
    /// </summary>
    List<IQuery> RefQueries { get; set; }
    /// <summary>
    /// 当前子查询最后AsCteTable后生成的对象，或是CTE表构建的子查询中的自引用对象，此时IsRecursive=true
    /// </summary>
    ICteQuery CteQueryObj { get; set; }
    bool IsRecursive { get; set; }
    string UnionSql { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    /// <summary>
    /// IncludeMany表，第二次执行时的参数列表，通常是Filter中使用的参数
    /// </summary>
    IDataParameterCollection NextDbParameters { get; set; }
    List<SqlFieldSegment> ReaderFields { get; set; }
    object RefFrom { get; set; }

    bool IsSecondUnion { get; set; }
    char TableAsStart { get; set; }
    int PageNumber { get; set; }
    int PageSize { get; set; }
    bool IsNeedCommandTableAlias { get; set; }
    bool IsNeedFetchShardingTables { get; }
    bool IsNeedFormatShardingTables { get; }
    bool IsNeedUnionShardingTables { get; }
    bool IsManyShardingTables { get; }
    string AggFieldAlias { get; set; }
    List<TableSegment> ShardingTables { get; set; }
    bool IsFromQuery { get; set; }
    bool IsFromCommand { get; set; }
    bool IsNeedPaging { get; set; }
    bool IsNeedFullFieldsPagingCount { get; set; }

    string BuildSql(bool isBuildCteSql, out List<SqlFieldSegment> readerFields);
    string BuildCommandSql(bool isBuildCteSql, out IDataParameterCollection dbParameters);
    string BuildShardingSql(string formatSql);
    string BuildCteTableSql(string tableName, out List<SqlFieldSegment> readerFields);

    string BuildTableShardingsSql();
    bool SetShardingTables(List<string> shardingTables);
    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTable(bool isIncludeMany, Func<string, bool> tableNamePredicate);
    void UseTableMap(bool isIncludeMany, Type masterEntityType, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, params object[] fieldValues);
    void UseTableByRange(bool isIncludeMany, object beginFieldValue, object endFieldValue);
    void UseTableByRange(bool isIncludeMany, object field1Value, object beginField2Value, object endField2Value);
    void UseTableByRange(bool isIncludeMany, object field1Value, object field2Value, object beginField3Value, object endField3Value);
    void UseTableSchema(bool isIncludeMany, string tableSchema);

    void From(char tableAsStart = 'a', params Type[] entityTypes);
    void AddTable(params Type[] entityTypes);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddJoinTable(Type entityType, string joinType = null, TableType tableType = TableType.Entity, string body = null, List<SqlFieldSegment> readerFields = null);
    TableSegment UseQuery(Type targetType, IQuery subQuery, bool isCopyRefParameters);
    TableSegment UseNewQuery(Type targetType, Expression subQueryExpr, bool isFirstTable);

    void Union(string union, Type targetType, IQuery subQuery);
    void Union(string union, Type targetType, Expression subQueryExpr);
    void UnionRecursive(string union, ICteQuery cteQueryObj, Expression selfSubQueryExpr);

    void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression subQueryExpr, Expression joinOn);

    bool Include(Expression memberSelector, Expression filter = null);
    bool ThenInclude(Expression memberSelector, Expression filter = null);
    bool HasIncludeTables();
    bool BuildIncludeSql(Type targetType, object target, bool isSingle, out string sql);
    void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isSingle);
    Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isSingle, CancellationToken cancellationToken);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
    void Or(Expression whereExpr);
    void GroupBy(Expression expr);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);

    void SelectGrouping();
    void SelectDefault(Expression defaultExpr);
    void Select(string sqlFormat, Expression selectExpr = null);
    void SelectFlattenTo(Type targetType, Expression specialMemberSelector = null);

    void Distinct();
    void Page(int pageNumber, int pageSize);
    void Skip(int skip);
    void Take(int limit);
    void AsCteTable(Type targetType, string tableName);
    void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<SqlFieldSegment> readerFields);
    void CopyShardingFromQueryVisitor(IQueryVisitor visitor);

    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    void Clear(bool isClearReaderFields = false);
    void CloneTo(IQueryVisitor visitor);
}