using System;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IDeleteVisitor : IDisposable
{
    string DbKey { get; }
    IDataParameterCollection DbParameters { get; set; }
    IOrmProvider OrmProvider { get; }
    IEntityMapProvider MapProvider { get; }

    void Initialize(Type entityType, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    string BuildCommand(IDbCommand command);
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    IDeleteVisitor WhereWith(object wherKeys);
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);
}