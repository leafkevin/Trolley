﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    bool IsFromCommand { get; set; }

    string BuildSql(out List<ReaderField> readerFields);
    string BuildCommandSql(Type targetType, out IDataParameterCollection dbParameters);
    string BuildCteTableSql(string tableName, out List<ReaderField> readerFields, out bool isRecursive);

    string BuildShardingTablesSql(string tableSchema);
    void SetShardingTables(List<string> shardingTables);
    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(Type entityType, Func<string, bool> tableNamePredicate);
    void UseTable(Type entityType, Type masterEntityType, Func<string, string, string, string, string> tableNameGetter);
    void UseTableBy(Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);

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

    void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    bool HasIncludeTables();
    bool BuildIncludeSql<TTarget>(Type targetType, TTarget target, out string sql);
    bool BuildIncludeSql<TTarget>(Type targetType, List<TTarget> targets, out string sql);
    void SetIncludeValues<TTarget>(Type targetType, TTarget target, IDataReader reader);
    Task SetIncludeValuesAsync<TTarget>(Type targetType, TTarget target, DbDataReader reader, CancellationToken cancellationToken);
    void SetIncludeValues<TTarget>(Type targetType, List<TTarget> targets, IDataReader reader);
    Task SetIncludeValueAsync<TTarget>(Type targetType, List<TTarget> targets, DbDataReader reader, CancellationToken cancellationToken);

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

    void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null);
    void RemoveTable(TableSegment tableSegment);
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    void Clear(bool isClearReaderFields = false);
}
