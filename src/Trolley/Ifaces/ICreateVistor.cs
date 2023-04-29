using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface ICreateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    ICreateVisitor From(Expression fieldSelector);
    ICreateVisitor Where(Expression whereExpr);
    ICreateVisitor And(Expression whereExpr); 
}
