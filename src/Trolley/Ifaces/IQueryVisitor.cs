using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQueryVisitor : IDisposable
{
    string DbKey { get; }
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    string WhereSql { get; }
    Dictionary<string, TableSegment> TableAliases { get; }
    /// <summary>
    /// 在解析子查询中，会用到父查询中的所有表，父查询中所有表别名引用
    /// </summary>
    Dictionary<string, TableSegment> RefTableAliases { get; set; }
    /// <summary>
    /// 在SQL查询中，引用到子查询或是CTE表对象，防止重复添加参数，同时也为了解析CTE表引用SQL
    /// </summary>
    List<IQuery> RefQueries { get; set; }
    ICteQuery SelfRefQueryObj { get; set; }
    bool IsRecursive { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    /// <summary>
    /// IncludeMany表，第二次执行时的参数列表，通常是Filter中使用的参数
    /// </summary>
    IDataParameterCollection NextDbParameters { get; set; }
    bool IsSecondUnion { get; set; }
    bool IsUseCteTable { get; set; }
    char TableAsStart { get; set; }
    int PageNumber { get; set; }
    int PageSize { get; set; }
    bool IsNeedFetchShardingTables { get; }
    List<TableSegment> ShardingTables { get; set; }
    bool IsFromQuery { get; set; }
    bool IsFromCommand { get; set; }
    bool IsUseMaster { get; }

    string BuildSql(out List<SqlFieldSegment> readerFields);
    string BuildCommandSql(out IDataParameterCollection dbParameters);
    string BuildCteTableSql(string tableName, out List<SqlFieldSegment> readerFields, out bool isRecursive);

    string BuildTableShardingsSql();
    void SetShardingTables(List<string> shardingTables);
    void UseTable(bool isIncludeMany, params string[] tableNames);
    void UseTable(bool isIncludeMany, Func<string, bool> tableNamePredicate);
    void UseTable(bool isIncludeMany, Type masterEntityType, Func<string, string, string, string> tableNameGetter);
    void UseTableBy(bool isIncludeMany, object field1Value, object field2Value = null);
    void UseTableByRange(bool isIncludeMany, object beginFieldValue, object endFieldValue);
    void UseTableByRange(bool isIncludeMany, object fieldValue1, object fieldValue2, object fieldValue3);
    void UseTableSchema(bool isIncludeMany, string tableSchema);
    void UseMaster(bool isUseMaster = true);

    void From(char tableAsStart = 'a', params Type[] entityTypes);
    void From(Type targetType, IQuery subQueryObj);
    void From(Type targetType, DbContext dbContext, Delegate subQueryGetter);

    void Union(string union, Type targetType, IQuery subQuery);
    void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter);
    void UnionRecursive(string union, DbContext dbContext, ICteQuery subQueryObj, Delegate selfSubQueryGetter);

    void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn);
    void Join(string joinType, Type newEntityType, DbContext dbContext, Delegate subQueryGetter, Expression joinOn);

    bool Include(Expression memberSelector, Expression filter = null);
    bool ThenInclude(Expression memberSelector, Expression filter = null);
    bool HasIncludeTables();
    bool BuildIncludeSql(Type targetType, object target, bool isSingle, out string sql);
    void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isSingle);
    Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isSingle, CancellationToken cancellationToken);
    void Where(Expression whereExpr);
    void And(Expression whereExpr);
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

    void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<SqlFieldSegment> readerFields);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<SqlFieldSegment> readerFields = null);
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    void Clear(bool isClearReaderFields = false);
}
