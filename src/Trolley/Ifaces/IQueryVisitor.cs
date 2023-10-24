using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IQueryVisitor
{
    //IDbCommand Command { get; set; }
    bool IsNeedAlias { get; set; }
    List<TableSegment> CteTables { get; set; }
    List<object> CteQueries { get; set; }
    Dictionary<object, TableSegment> CteTableSegments { get; set; }
    List<IDbDataParameter> DbParameters { get; set; }

    string BuildSql(out List<ReaderField> readerFields, bool isContainsCteSql = true, bool isUnion = false);
    bool HasIncludeTables();
    bool BuildIncludeSql(object parameter, out string sql);
    void SetIncludeValues(object parameter, IDataReader reader);
    void From(char tableAsStart, params Type[] entityTypes);
    void From(char tableAsStart, Type entityType, string suffixRawSql);
    TableSegment WithTable(Type entityType, string rawSql, List<ReaderField> readerFields, bool isUnion = false, object queryObject = null, bool isRecursive = false);
    TableSegment WithTable(Type entityType, string rawSql, List<ReaderField> readerFields, string cteTableName, object queryObject);
    void BuildCteTable(string cteTableName, string rawSql, List<ReaderField> readerFields, object cteQuery, bool isClear = false);
    void Union(TableSegment tableSegment, string rawSql);
    void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void Join(string joinType, Expression joinOn);
    void Join(string joinType, TableSegment joinTableSegment, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void JoinCteTable(string joinType, string cteTableName, Expression joinOn);
    void Select(string sqlFormat, Expression selectExpr = null, bool isFromQuery = false);
    void SelectGrouping(bool isFromQuery = false);
    void SelectDefault(Expression defaultExpr);
    void GroupBy(Expression expr);
    void OrderBy(string orderBy);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);
    void Page(int pageIndex, int pageSize);
    void Skip(int skip);
    void Take(int limit);
    void Where(Expression whereExpr, bool isClearTableAlias = true);
    void And(Expression whereExpr);
    void Distinct();
    void InsertTo(Type entityType);
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null);
    void AddAliasTable(string aliasName, TableSegment tableSegment);
    void Clear(bool isClearTables = false, bool isClearReaderFields = false);
    void CopyTo(IQueryVisitor visitor);
}
