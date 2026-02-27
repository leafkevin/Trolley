using System.Data;

namespace Trolley;

public interface ICommandContext
{
    ITheaCommand Command { get; set; }
    IDataParameterCollection DbParameters { get; set; }
}