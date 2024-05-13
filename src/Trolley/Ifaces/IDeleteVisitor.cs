using System;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IDeleteVisitor : IDisposable
{
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }

    void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(IDbCommand command);
    void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);

    #region Sharding
    void UseTable(Type entityType, params string[] tableNames);
    void UseTable(IOrmDbFactory dbFactory, Type entityType, Func<string, bool> tableNamePredicate);
    void UseTableBy(IOrmDbFactory dbFactory, Type entityType, object field1Value, object field2Value = null);
    void UseTableByRange(IOrmDbFactory dbFactory, Type entityType, object beginFieldValue, object endFieldValue);
    void UseTableByRange(IOrmDbFactory dbFactory, Type entityType, object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    IDeleteVisitor WhereWith(object wherKeys);
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);
}