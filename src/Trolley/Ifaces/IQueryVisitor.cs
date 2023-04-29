using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IQueryVisitor
{
    /// <summary>
    /// Union,ToSql,Join,Cte，各种子查询,各种单值查询，但都有SELECT操作
    /// </summary>
    /// <param name="dbParameters"></param>
    /// <param name="readerFields"></param>
    /// <returns></returns>
    string BuildSql(out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields, bool isUnion = false);
    /// <summary>
    /// First,ToList,ToPageList接口使用,First,ToList有转换其他类型时，entityType，toTargetExpr两个栏位有值
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="toTargetExpr"></param>
    /// <param name="dbParameters"></param>
    /// <param name="readerFields"></param>
    /// <returns></returns>
    string BuildSql(Type targetType, Expression toTargetExpr, out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields, out bool isTarget);
    bool BuildIncludeSql(object parameter, out string sql);
    void SetIncludeValues(object parameter, IDataReader reader);
    QueryVisitor From(params Type[] entityTypes);
    QueryVisitor From(char tableAsStart, params Type[] entityTypes);
    QueryVisitor From(char tableAsStart, Type entityType, string suffixRawSql);
    TableSegment WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null, string joinType = "");
    QueryVisitor WithCteTable(Type entityType, string cteTableName, bool isRecursive, string rawSql, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null);
    void Union(string body, List<ReaderField> readerFields, List<IDbDataParameter> dbParameters = null);
    void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null);
    void Join(string joinType, Expression joinOn);
    void Join(string joinType, Type newEntityType, Expression joinOn);
    void Join(string joinType, TableSegment joinTableSegment, Expression joinOn);
    void Join(string joinType, Type newEntityType, string cteTableName, Expression joinOn);
    void Select(string sqlFormat, Expression selectExpr = null, bool isFromQuery = false);
    void SelectGrouping(bool isFromQuery = false);
    void DefaultSelect(Expression defaultExpr);
    void GroupBy(Expression expr);
    void OrderBy(string orderBy);
    void OrderBy(string orderType, Expression expr);
    void Having(Expression havingExpr);
    QueryVisitor Page(int pageIndex, int pageSize);
    QueryVisitor Skip(int skip);
    QueryVisitor Take(int limit);
    QueryVisitor Where(Expression whereExpr, bool isClearTableAlias = true);
    QueryVisitor And(Expression whereExpr);
    void Distinct();
    TableSegment InitTableAlias(LambdaExpression lambdaExpr);
    TableSegment AddTable(TableSegment tableSegment);
    TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Master, string body = null, List<ReaderField> readerFields = null);
}
