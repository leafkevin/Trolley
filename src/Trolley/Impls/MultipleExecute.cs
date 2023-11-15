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
public struct FieldObject
{
    public Expression FieldSelector { get; set; }
    public object FieldValue { get; set; }
}
public struct FieldsParameters
{
    public Expression SelectorOrAssignment { get; set; }
    public object Parameters { get; set; }
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
    public object DbParametersInitializer { get; set; }
    public object BulkObjects { get; set; }
}