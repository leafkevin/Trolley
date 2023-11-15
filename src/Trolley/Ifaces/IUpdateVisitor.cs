using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public enum UpdateFieldType
{
    /// <summary>
    /// 设置字段
    /// </summary>
    SetField,
    /// <summary>
    /// 设置NULL
    /// </summary>
    SetValue,
    Where
}
public struct UpdateField
{
    public UpdateFieldType Type { get; set; }
    public MemberMap MemberMapper { get; set; }
    public string Value { get; set; }
    public override int GetHashCode() => HashCode.Combine(this.Type, this.MemberMapper?.MemberName);
}
public interface IUpdateVisitor
{
    string DbKey { get; }
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }

    void Initialize(Type entityType, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(IDbCommand command);
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    string BuildSql();
    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsAssignment);
    IUpdateVisitor Set(Expression fieldSelector, object fieldValue);
    IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj);
    IUpdateVisitor SetFrom(Expression fieldsAssignment);
    IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector);
    IUpdateVisitor SetBulkFirst(IDbCommand command, Expression fieldsSelectorOrAssignment, object updateObjs);
    void SetBulkHead(StringBuilder builder);
    void SetBulk(StringBuilder builder, object updateObj, int index);
    void SetBulkTail(StringBuilder builder);
    IUpdateVisitor WhereWith(object whereObj);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
