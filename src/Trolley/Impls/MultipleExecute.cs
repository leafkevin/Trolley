using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
    public object Body { get; set; }
    public TaskCompletionSource Waiter { get; set; }
}
public struct FieldObject
{
    public Expression Selector { get; set; }
    public object Value { get; set; }
}
public struct BulkObject
{
    public string HeadSql { get; set; }
    public object CommandInitializer { get; set; }
    public IEnumerable BulkObjects { get; set; }
}
public enum DeferredInsertType
{
    WithBy,
    WithBulk,
    WithByField,
    SetObject,
    SetExpression,
}
public struct InsertDeferredSegment
{
    public DeferredInsertType Type { get; set; }
    public object Value { get; set; }
}
public struct InsertDeferredCommand
{
    public Type EntityType { get; set; }
    public List<InsertDeferredSegment> Segments { get; set; }
}