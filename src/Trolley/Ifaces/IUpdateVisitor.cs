using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IUpdateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    UpdateVisitor From(params Type[] entityTypes);
    UpdateVisitor Join(string joinType, Type entityType, Expression joinOn);
    UpdateVisitor Set(Expression fieldsExpr, object fieldValue = null);
    SetField SetValue(MemberMap memberMapper, Expression valueExpr, out bool hasParameterFields);
    SetField SetValue(Expression fieldsExpr, MemberMap memberMapper, Expression valueExpr, out bool hasParameterFields);
    UpdateVisitor SetFromQuery(Expression fieldsExpr, Expression valueExpr = null);
    UpdateVisitor Where(Expression whereExpr);
    UpdateVisitor And(Expression whereExpr);
}
