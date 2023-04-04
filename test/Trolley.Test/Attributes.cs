using System;

namespace Trolley.Test;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableAttribute : Attribute
{
    public string TableName { get; set; }
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class KeyAttribute : Attribute
{
    public string FieldName { get; set; }
    public string DbType { get; set; }
    public bool IsIdentity { get; set; }
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FieldAttribute : Attribute
{
    public string FieldName { get; set; }
    public string DbType { get; set; }
    public bool AutoIncrement { get; set; }
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TypeHandlerAttribute : Attribute
{
    public Type HandlerType { get; set; }
    public TypeHandlerAttribute(Type typeHandler)
        => this.HandlerType = typeHandler;
}
[AttributeUsage(AttributeTargets.Property)]
public class ReferenceAttribute : Attribute
{
}
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class IgnoreAttribute : Attribute
{
}