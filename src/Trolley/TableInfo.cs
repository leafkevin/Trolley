namespace Trolley;

public class TableInfo
{
    public string TableName { get; set; }
    public string Description { get; set; }
}
public class ColumnInfo
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public string ColumnType { get; set; }
    public ulong Length { get; set; }
    public int Scale { get; set; }
    public int Precision { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsIdentity { get; set; }
    public bool IsNullable { get; set; }
    public string Description { get; set; }
    public string DefaultValue { get; set; }
    public int Position { get; set; }
}
