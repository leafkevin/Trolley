using System.Data;

namespace Trolley;

public interface ICommandVisitor
{
    ITheaCommand Command { get; set; }
    IDataParameterCollection DbParameters { get; set; }
    string BuildSql(out ReusableList<ReaderField> readerFields);
}