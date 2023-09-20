using System;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor
{
    IDbCommand Command { get; set; }
    string BuildCommand(IDbCommand command);
    MultipleCommand CreateMultipleCommand();
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isFirst = true);
    string BuildHeadSql();
    string BuildTailSql();
    ICreateVisitor UseIgnore();
    ICreateVisitor IfNotExists(object whereObj);
    ICreateVisitor IfNotExists(Expression keysPredicate);
    //ICreateVisitor Set(Expression fieldsAssignment);
    //ICreateVisitor Set(object updateObj);
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithByField(FieldObject fieldObject);
    ICreateVisitor WithBulkFirst(IDbCommand command, IEnumerable insertObjs);
    ICreateVisitor WithBulk(StringBuilder builder, object insertObj, int index);
    IQueryVisitor CreateQuery(params Type[] sourceTypes);
}
