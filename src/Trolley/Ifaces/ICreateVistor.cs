using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor
{
    string BuildSql(out List<IDbDataParameter> dbParameters);
    string BuildHeadSql();
    string BuildTailSql();
    ICreateVisitor UseIgnore();
    ICreateVisitor IfNotExists(object whereObj);
    ICreateVisitor IfNotExists(Expression keysPredicate);
    ICreateVisitor Set(Expression fieldsAssignment);
    ICreateVisitor Set(object updateObj);
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithBy(Expression fieldSelector, object fieldValue);
    ICreateVisitor WithBulkFirst(object insertObjs);
    ICreateVisitor WithBulk(IDbCommand command, StringBuilder builder, int index, object insertObj);
    IQueryVisitor CreateQuery(params Type[] sourceTypes);
}
