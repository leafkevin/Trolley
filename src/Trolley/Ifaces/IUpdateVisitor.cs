using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IUpdateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    IUpdateVisitor From(params Type[] entityTypes);
    IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    IUpdateVisitor Set(Expression fieldsExpr, object fieldValue = null);
    SetField SetValue(MemberMap memberMapper, Expression valueExpr );
    SetField SetValue(Expression fieldsExpr, MemberMap memberMapper, Expression valueExpr );
    IUpdateVisitor SetFromQuery(Expression fieldsExpr, Expression valueExpr = null);
    IUpdateVisitor Where(Expression whereExpr);
    IUpdateVisitor And(Expression whereExpr);
}
