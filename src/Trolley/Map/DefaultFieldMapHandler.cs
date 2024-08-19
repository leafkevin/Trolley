using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Trolley;

public class DbTableInfo
{
    public string TableSchema { get; set; }
    public string TableName { get; set; }
    public List<DbColumnInfo> Columns { get; set; }
}
public class DbColumnInfo
{
    public string FieldName { get; set; }
    public string DataType { get; set; }
    public int ArrayDimens { get; set; }
    public string DbColumnType { get; set; }
    public int MaxLength { get; set; }
    public int Scale { get; set; }
    public int Precision { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public bool IsNullable { get; set; }
    public string Description { get; set; }
    public string DefaultValue { get; set; }
    public int Position { get; set; }
}
public class DefaultFieldMapHandler : IFieldMapHandler
{
    public bool TryFindMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo)
    {
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));

        memberInfo = memberInfos.Find(f => f.Name == fieldName);
        if (memberInfo != null)
            return true;
        memberInfo = memberInfos.Find(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (memberInfo != null)
            return true;
        fieldName = fieldName.Replace("_", string.Empty);
        memberInfo = memberInfos.Find(f => f.Name == fieldName);
        if (memberInfo != null)
            return true;
        memberInfo = memberInfos.Find(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        if (memberInfo != null)
            return true;
        return false;
    }
    public bool TryFindField(string memberName, List<string> fieldNames, out string fieldName)
    {
        if (string.IsNullOrEmpty(memberName))
            throw new ArgumentNullException(nameof(memberName));

        fieldName = fieldNames.Find(f => f == memberName);
        if (fieldName != null)
            return true;
        fieldName = fieldNames.Find(f => f.Equals(memberName, StringComparison.OrdinalIgnoreCase));
        if (fieldName != null)
            return true;
        var myFieldNames = fieldNames.Select(f => f.Replace("_", string.Empty)).ToList();
        fieldName = myFieldNames.Find(f => f == memberName);
        if (fieldName != null)
            return true;
        fieldName = myFieldNames.Find(f => f.Equals(memberName, StringComparison.OrdinalIgnoreCase));
        if (fieldName != null)
            return true;
        return false;
    }
}
