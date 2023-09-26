using System;
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
    string BuildSql();
    string BuildHeadSql();
    string BuildTailSql();
    ICreateVisitor UseIgnore();
    ICreateVisitor IfNotExists(object whereObj);
    ICreateVisitor IfNotExists(Expression keysPredicate);
    //ICreateVisitor Set(Expression fieldsAssignment);
    //ICreateVisitor Set(object updateObj);
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithByField(FieldObject fieldObject);
    Action<IDbCommand, StringBuilder, object, int> WithBulkFirst(IDbCommand command, object insertObjs);
    void WithBulkHead(StringBuilder builder);
    void WithBulk(StringBuilder builder, Action<IDbCommand, StringBuilder, object, int> commandInitializer, object insertObj, int index);
    void WithBulkTail(StringBuilder builder);
    IQueryVisitor CreateQuery(params Type[] sourceTypes);
}
