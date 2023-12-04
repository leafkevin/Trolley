using System;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface ICreateVisitor : IDisposable
{
    string DbKey { get; }
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }
    bool IsBulk { get; set; }

    string BuildCommand(IDbCommand command, bool isReturnIdentity);
    MultipleCommand CreateMultipleCommand();
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isFirst = true);
    string BuildSql();
    ICreateVisitor WithBy(object insertObj);
    ICreateVisitor WithByField(Expression fieldSelector, object fieldValue);
    ICreateVisitor WithBulk(object insertObjs, int bulkCount);
    (IEnumerable, int, Action<StringBuilder>, Action<StringBuilder, object, string>) BuildWithBulk(IDbCommand command);
    ICreateVisitor WithFrom<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery, string cteTableName = null);
    ICreateVisitor IgnoreFields(string[] fieldNames);
    ICreateVisitor IgnoreFields(Expression fieldsSelector);
    ICreateVisitor OnlyFields(string[] fieldNames);
    ICreateVisitor OnlyFields(Expression fieldsSelector);
}
