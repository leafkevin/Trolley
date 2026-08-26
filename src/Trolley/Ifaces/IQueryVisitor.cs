using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQueryVisitor : ICommandVisitor, ICloneable, IDisposable
{
    DbContext DbContext { get; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider EntityMapProvider { get; }
    List<TableSegment> Tables { get; set; }
    ITableShardingProvider ShardingProvider { get; }

    /// <summary>
    /// IncludeMany表，第二次执行时的参数列表，通常是Filter中使用的参数
    /// </summary>
    IDataParameterCollection NextDbParameters { get; set; }
    List<ReaderField> ReaderFields { get; set; }

    StringBuilder WhereBuilder { get; }
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

    bool IsSecondUnion { get; set; }
    char TableAliasStart { get; set; }
    int PageNumber { get; }
    int PageSize { get; }
    bool IsManyShardingTables { get; }
    /// <summary>
    /// 当有多个分表时，当有GROUP BY/ORDER BY/LIMIT/SUM/AVG/MAX/MIN等操作时，就需要UNION多个分表查询结果，
    /// 在最外层再进行一次GROUP BY/ORDER BY/LIMIT、SUM/AVG/MAX/MIN等操作
    /// </summary>
    bool IsNeedChangeUnionShardingTables { get; }
    List<TableSegment> ShardingTables { get; set; }
    string ShardingTableJointMark { get; set; }
    bool IsNeedPaging { get; set; }
    bool IsScalar { get; set; }
    bool IsRefQuery { get; set; }
    string RefSql { get; set; }


    string BuildSql(bool isBuildCteSql, out List<ReaderField> readerFields);
    string BuildCommandSql(Type entityType, out IDataParameterCollection dbParameters);
    string BuildShardingTablesSqlByFormat(string formatSql, string jointMark);
    string BuildShardingSql(string formatSql);
    string BuildShardingScalarSql(string formatSql);
    string BuildCteTableSql(string tableName, out List<ReaderField> readerFields);

    void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames);
    void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues);
    void UseTableMap(TableShardingUsageMode usageMode, bool isIncludeMany, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues);
    void UseUnionShardingTable();
    void UseTableSchema(bool isIncludeMany, string tableSchema);
    void WithTableAliasTrailing(bool isIncludeMany, string rawSql);

    void From(char tableAsStart = 'a', params Type[] entityTypes);
    void AddTable(params Type[] entityTypes);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment UseQuery(Type targetType, IQuery subQuery, bool isClearTables);
    void UseNewQuery(Type targetType, Expression subQueryExpr, bool isClearTables);

    void Union(string union, Type targetType, IQuery subQuery);
    void Union(string union, Type targetType, Expression subQueryExpr);
    void UnionRecursive(string union, Type targetType, Expression selfSubQueryExpr);

    void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression subQueryExpr, Expression joinOn);

    bool Include(Expression memberSelector, Expression filter = null);
    bool ThenInclude(Expression memberSelector, Expression filter = null);
    bool HasIncludeTables();
    bool BuildIncludeSql(Type targetType, object target, bool isMultiResult, out string sql);
    void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isMultiResult);
    Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isMultiResult, CancellationToken cancellationToken);

    void AndBy(object whereObj);
    void AndById(object whereKey);
    void AndByIds(object whereKeys);
    void And(Expression whereExpr);
    void OrBy(object whereObj);
    void OrById(object whereKey);
    void OrByIds(object whereKeys);
    void Or(Expression whereExpr);

    void GroupBy(Expression expr);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);

    void SelectGrouping();
    void SelectDefault(Expression defaultExpr);
    void SelectRaw(Type targetType, string rawFields, string aggFunc = null);
    void Select(Expression selectExpr);
    void Select(string sqlFormat, Expression selectExpr);
    void SelectTo(Type targetType, Expression specialMemberSelector = null);

    void Distinct();
    void Page(int pageNumber, int pageSize);
    void Skip(int skip);
    void Take(int limit);
    void AsCteTable(Type targetType, string tableName);

    void WithLeadingSql(string rawSql);
    void WithTrailingSql(string rawSql);

    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    List<ReaderField> FlattenTableFields(TableSegment tableSegment, bool isNeedAlias = true);
    void Clear(bool isClearReaderFields = false);
    void CloneTo(IQueryVisitor visitor);
}