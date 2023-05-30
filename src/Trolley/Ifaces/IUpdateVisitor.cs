using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public class SetField
{
    public MemberMap MemberMapper { get; set; }
    public string Value { get; set; }
}
public interface IUpdateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsExpr);
    IUpdateVisitor Set(Expression fieldsExpr, object fieldValue);
    string WithBy(Expression fieldsExpr, object parameters, out List<IDbDataParameter> dbParameters);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
