using System;
using System.Collections.Generic;
using System.Text;

namespace Trolley;

public class DbTableInfo
{
    public string TableSchema { get; set; }
    public string TableName { get; set; }
    public List<DbColumnInfo> Columns { get; set; }
}
public class DbColumnInfo
{
    public string TableName { get; set; }
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
