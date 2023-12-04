using System;
using System.Linq.Expressions;

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
public struct FieldFromQuery
{
    public Expression FieldSelector { get; set; }
    public Expression ValueSelector { get; set; }
}
public struct BulkObject
{
    public string HeadSql { get; set; }
    public Expression FieldsSelectorOrAssignment { get; set; }
    public object CommandInitializer { get; set; }
    public object BulkObjects { get; set; }
}