using System;
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
    bool IsMultiple { get; set; }
    int CommandIndex { get; set; }
    List<TableSegment> CteTables { get; set; }
    List<object> CteQueries { get; set; }
    Dictionary<object, TableSegment> CteTableSegments { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    TableSegment SelfTableSegment { get; set; }

    string BuildSql(out List<ReaderField> readerFields, bool hasCteSql = true, bool isUnion = false);

    void From(char tableAsStart = 'a', string suffixRawSql = null, params Type[] entityTypes);
    void From(Type targetType, IQueryBase subQueryObj);
    void From(Type targetType, DbContext dbContext, Delegate subQueryGetter);

    void FromWithFirst(Type targetType, Func<IQueryBase> cteQueryObjGetter);
    void FromWithFirst(Type targetType, DbContext dbContext, Delegate cteSubQueryGetter);
    void NextWith(Type targetType, DbContext dbContext, Delegate cteSubQueryGetter);

    void Union(string union, Type targetType, IQueryBase subQuery);
    void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter);
    void UnionRecursive(string union, Type targetType, DbContext dbContext, IQueryBase subQueryObj, Delegate selfSubQueryGetter);
    TableSegment UseTable(Type targetType, string rawSql, List<ReaderField> readerFields, object queryObj, bool isUnion);

    public void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, Type newEntityType, IQueryBase subQuery, Expression joinOn);
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

    void Where(Expression whereExpr, bool isClearTableAlias = true);
    void And(Expression whereExpr);
    void GroupBy(Expression expr);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);
    void SelectDefault(Expression defaultExpr);
    void Select(string sqlFormat, Expression selectExpr = null);

    void Distinct();
    void Page(int pageIndex, int pageSize);
    void Skip(int skip);
    void Take(int limit);

    void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null);
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    void Clear(bool isClearTables = false, bool isClearReaderFields = false);
    void CopyTo(IQueryVisitor visitor);
}
