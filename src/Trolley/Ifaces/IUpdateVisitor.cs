using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public enum UpdateFieldType
{
    SetField,
    SetValue,
    RawSql,
    Where
}
public struct UpdateField
{
    public UpdateFieldType Type { get; set; }
    public MemberMap MemberMapper { get; set; }
    public string Value { get; set; }
}
public interface IUpdateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsAssignment);
    IUpdateVisitor Set(Expression fieldSelector, object fieldValue);
    IUpdateVisitor SetRaw(string rawSql, object parameters);
    IUpdateVisitor SetWith(Expression fieldsAssignment);
    IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj, bool isExceptKey = false);
    IUpdateVisitor SetFrom(Expression fieldsAssignment);
    IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector);
    IUpdateVisitor SetBulkFirst(Expression fieldsSelectorOrAssignment, object updateObjs);
    void SetBulk(StringBuilder builder, IDbCommand command, object updateObj, int index);
    IUpdateVisitor WhereWith(object whereObj, bool isOnlyKeys = false);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
