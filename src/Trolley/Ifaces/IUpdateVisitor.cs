using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

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
    IUpdateVisitor SetValue(Expression fieldsExpr, object fieldValue);
    string WithBy(Expression fieldsExpr, object parameters, out List<IDbDataParameter> dbParameters);
    List<IDbDataParameter> WithBulkBy(Expression fieldsExpr, StringBuilder builder, object parameters, int index, out List<IDbDataParameter> fixedDbParameters);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
