using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IDeleteVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    void Initialize(Type entityType, bool isFirst = true);
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);
}