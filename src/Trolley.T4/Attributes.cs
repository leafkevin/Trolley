using System;

namespace Trolley.T4;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableAttribute : Attribute
{
    public string TableName { get; set; }
}
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute
{
    public string FieldName { get; set; }
    public string DbType { get; set; }
    public bool IsIdentity { get; set; }
}
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    public string FieldName { get; set; }
    public string DbType { get; set; }
    public bool AutoIncrement { get; set; }
}
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
}