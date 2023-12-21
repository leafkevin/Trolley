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
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    void Initialize(Type entityType, bool isFirst = true);
    string BuildSql();
    void WithBy(object insertObj);
    void WithByField(Expression fieldSelector, object fieldValue);
    void WithBulk(object insertObjs, int bulkCount);
    (IEnumerable, int, Action<StringBuilder>, Action<StringBuilder, object, string>) BuildWithBulk(IDbCommand command);
    void WithFrom<TTarget>(IQuery<TTarget> cteSubQuery, bool isUseCte = false);
    void WithFrom<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery, bool isUseCte = false);
    void IgnoreFields(string[] fieldNames);
    void IgnoreFields(Expression fieldsSelector);
    void OnlyFields(string[] fieldNames);
    void OnlyFields(Expression fieldsSelector);
}
