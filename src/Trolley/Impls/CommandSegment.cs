using System.Collections.Generic;
using System.Data;

namespace Trolley;

public struct CommandSegment
{
    public string Type { get; set; }
    public object Value { get; set; }
}
public struct BulkSqlSegment
{
    /// <summary>
    /// INSERT INTO scheam. 或是 UPDATE scheam. 之类的前段SQL
    /// </summary>
    public string HeadSql { get; set; }
    /// <summary>
    /// (Field1, Field2, Field3) VALUES之类的首次不可循环SQL
    /// </summary>
    public string FixedFieldsSql { get; set; }
    /// <summary>
    /// (@Field1, @Field2, @Field3, 或是 Field1=@Field1, Field2=@Field2 WHERE Field1=@xxx, Field2=@xxx AND 之类的可循环SQL
    /// </summary>
    public string FixedValuesSql { get; set; }
    /// <summary>
    /// 首次不可循环的参数列表
    /// </summary>
    public List<IDbDataParameter> FixedDbParameters { get; set; }
}