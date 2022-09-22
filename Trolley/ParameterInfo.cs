using System;

namespace Trolley;

struct ParameterInfo
{
    public bool IsMulti { get; set; }
    public bool IsDictionary { get; set; }
    public Type ParameterType { get; set; }
    public object Parameters { get; set; }
    public int? MulitIndex { get; set; }
}
