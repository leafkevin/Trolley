using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IQueryVisitor
{
    bool IsNeedAlias { get; set; }
    char TableAsStart { get; set; }
    IOrmProvider OrmProvider { get; }
    /// <summary>
    /// Union,ToSql,Join,Cte，各种子查询,各种单值查询，但都有SELECT操作
    /// </summary>
    /// <param name="dbParameters"></param>
    /// <param name="readerFields"></param>
    /// <returns></returns>
    string BuildSql(out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields, bool isUnion = false, char unionAlias = 'a');
    bool BuildIncludeSql(object parameter, out string sql);
    void SetIncludeValues(object parameter, IDataReader reader);
    IQueryVisitor From(params Type[] entityTypes);
    IQueryVisitor From(char tableAsStart, params Type[] entityTypes);
    IQueryVisitor From(char tableAsStart, Type entityType, string suffixRawSql);
    TableSegment WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null, string joinType = "");
    IQueryVisitor WithCteTable(Type entityType, string cteTableName, bool isRecursive, string rawSql, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null);
    void Union(string body, List<ReaderField> readerFields, List<IDbDataParameter> dbParameters = null, char tableAlias = 'a');
    void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, TableSegment joinTableSegment, Expression joinOn);
    void Join(string joinType, Type newEntityType, string cteTableName, Expression joinOn);
    void Select(string sqlFormat, Expression selectExpr = null, bool isFromQuery = false);
    void SelectGrouping(bool isFromQuery = false);
    void SelectDefault(Expression defaultExpr);
    void GroupBy(Expression expr);
    void OrderBy(string orderBy);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);
    IQueryVisitor Page(int pageIndex, int pageSize);
    IQueryVisitor Skip(int skip);
    IQueryVisitor Take(int limit);
    IQueryVisitor Where(Expression whereExpr, bool isClearTableAlias = true);
    IQueryVisitor And(Expression whereExpr);
    void Distinct();
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null);
    void AddAliasTable(string aliasName, TableSegment tableSegment);
    IQueryVisitor Clone(char tableAsStart = 'a', string parameterPrefix = "p");
}
