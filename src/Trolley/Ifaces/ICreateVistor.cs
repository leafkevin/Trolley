using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor
{
    string DbKey { get; }
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }

    string BuildCommand(IDbCommand command);
    MultipleCommand CreateMultipleCommand();
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isFirst = true);
    string BuildSql();
    string BuildHeadSql();
    string BuildTailSql();
    //ICreateVisitor IfNotExists(object whereObj);
    //ICreateVisitor IfNotExists(Expression keysPredicate);  
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithByField(FieldObject fieldObject);
    ICreateVisitor WithBulk(object insertObjs);
    string BuildBulkHeadSql(StringBuilder builder, out object commandInitializer);
    void WithBulk(IDbCommand command, StringBuilder builder, Action<IDataParameterCollection, StringBuilder, object, int> dbParametersInitializer, object insertObj, int index);
    void WithBulkTail(StringBuilder builder);
    IQueryVisitor CreateQuery(params Type[] sourceTypes);
}
