using System;
using System.Collections.Generic;

namespace Trolley;

public enum MultipleCommandType
{
    Insert,
    Update,
    Delete
}
public struct MultipleCommand
{
    public MultipleCommandType CommandType { get; set; }
    public Type EntityType { get; set; }
    public object Body { get; set; }
    public List<TableSegment> Tables { get; set; }
    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public List<IQuery> RefQueries { get; set; }
    public bool IsNeedTableAlias { get; set; }
    public bool IsJoin { get; set; }
}
public struct CommandSegment
{
    public string Type { get; set; }
    public object Value { get; set; }
}
public struct FieldsSegment
{
    public string Type { get; set; }
    public string Fields { get; set; }
    public string Values { get; set; }
}