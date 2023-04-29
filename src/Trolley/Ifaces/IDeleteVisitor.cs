using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IDeleteVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    DeleteVisitor Where(Expression whereExpr);
    DeleteVisitor And(Expression whereExpr);
}