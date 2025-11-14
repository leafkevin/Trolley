using System;
using System.Collections.Generic;

namespace Trolley;

public struct CommandSegment
{
    public string Type { get; set; }
    public object Value { get; set; }
}
struct ValueFieldSegment
{
    public MemberMap MemberMapper { get; set; }
    public Func<object, object> ValueGetter { get; set; }
    public ValueFieldSegment(MemberMap memberMapper, Func<object, object> valueGetter)
    {
        MemberMapper = memberMapper;
        ValueGetter = valueGetter;
    }
}