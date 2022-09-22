using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class ReaderFieldInfo
{
    public int Index { get; set; }
    public bool IsTarget { get; set; } = true;
    public MemberInfo Member { get; set; }
    public Type FromType { get; set; }
    public EntityMap RefMapper { get; set; }
    public string MemberName { get; set; }
    public Expression Expression { get; set; }
    public bool IsIncluded { get; set; }
}
