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
    bool IsUseFieldAlias { get; set; }
    bool IsUseCteTable { get; set; }
    char TableAsStart { get; set; }
    int PageIndex { get; set; }
    int PageSize { get; set; }
    /// <summary>
    /// 解析子查询时，父亲查询的TableAliases
    /// </summary>
    Dictionary<string, TableSegment> RefTableAliases { get; set; }

    string BuildSql(out List<ReaderField> readerFields);
    string BuildCommandSql(Type targetType, out IDataParameterCollection dbParameters);
    string BuildCteTableSql(string tableName, out List<ReaderField> readerFields, out bool isRecursive);
    void From(char tableAsStart = 'a', string suffixRawSql = null, params Type[] entityTypes);
    void From(Type targetType, IQuery subQueryObj);
    void From(Type targetType, DbContext dbContext, Delegate subQueryGetter);

    void Union(string union, Type targetType, IQuery subQuery);
    void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter);
    void UnionRecursive(string union, DbContext dbContext, ICteQuery subQueryObj, Delegate selfSubQueryGetter);

    public void Join(string joinType, Expression joinOn);
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
    void Select(string sqlFormat, Expression selectExpr = null, bool isFromCommand = false);
    void SelectFlattenTo(Type targetType, Expression specialMemberSelector = null);

    void Distinct();
    void Page(int pageIndex, int pageSize);
    void Skip(int skip);
    void Take(int limit);

    void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null);
    void RemoveTable(TableSegment tableSegment);
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    void Clear(bool isClearReaderFields = false);
}
