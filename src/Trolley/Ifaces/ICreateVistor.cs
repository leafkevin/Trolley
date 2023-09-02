using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor
{
    string DbKey { get; }
    IEntityMapProvider MapProvider { get; }
    IOrmProvider OrmProvider { get; }
    bool IsParameterized { get; set; }
    char TableAsStart { get; set; }
    bool IsNeedAlias { get; set; }

    string BuildSql(out List<IDbDataParameter> dbParameters);
    string BuildHeadSql();
    string BuildTailSql();
    ICreateVisitor UseIgnore();
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithBy(Expression fieldSelector, object fieldValue);
    ICreateVisitor WithBulkFirst(object insertObjs);
    ICreateVisitor WithBulk(IDbCommand command, StringBuilder builder, int index, object insertObj);
    ICreateVisitor From(Expression fieldSelector);
    ICreateVisitor Where(Expression whereExpr);
    ICreateVisitor And(Expression whereExpr);
}
