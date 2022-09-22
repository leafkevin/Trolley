using System;

namespace Trolley;

public class FromTableInfo
{
    public Type EntityType { get; set; }
    public EntityMap Mapper { get; set; }
    public string AlaisName { get; set; }
    public string JoinType { get; set; }
    public string JoinOn { get; set; }
}