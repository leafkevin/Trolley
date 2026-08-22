using System.Collections.Generic;
using System.Data;

namespace Trolley;

public interface ICommandVisitor
{
    ITheaConnection Connection { get; set; }
    ITheaCommand Command { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    (bool, ITheaConnection, ITheaCommand) UseCommand();
    string BuildSql(out List<ReaderField> readerFields);
}