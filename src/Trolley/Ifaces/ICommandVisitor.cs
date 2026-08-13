using System.Collections.Generic;
using System.Data;

namespace Trolley;

public interface ICommandVisitor
{
    ITheaCommand Command { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    string BuildSql(ITheaCommand command, out List<ReaderField> readerFields);
}