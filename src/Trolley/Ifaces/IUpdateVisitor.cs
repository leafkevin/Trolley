using System;
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
    public override int GetHashCode()
        => HashCode.Combine(this.Type, this.MemberMapper, this.Value);
}
public interface IUpdateVisitor
{
    string BuildSql();
    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsAssignment);
    IUpdateVisitor Set(Expression fieldSelector, object fieldValue);
    IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj);
    IUpdateVisitor SetFrom(Expression fieldsAssignment);
    IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector);
    IUpdateVisitor SetBulkFirst(Expression fieldsSelectorOrAssignment, object updateObjs);
    void SetBulk(StringBuilder builder, IDbCommand command, object updateObj, int index);
    IUpdateVisitor WhereWith(object whereObj);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
