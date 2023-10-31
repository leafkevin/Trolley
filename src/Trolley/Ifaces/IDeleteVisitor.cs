using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public interface IDeleteVisitor
{
    IDataParameterCollection DbParameters { get; set; }

    string BuildSql(out List<IDbDataParameter> dbParameters);
    void Initialize(Type entityType, bool isFirst = true);
    MultipleCommand CreateMultipleCommand();
    int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex);
    string BuildSql();
    IDeleteVisitor Where(Expression whereExpr);
    IDeleteVisitor And(Expression whereExpr);
}