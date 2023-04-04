namespace Trolley;

public enum ReaderFieldType : byte
{
    Field = 1,
    Entity = 2,
    /// <summary>
    /// 临时的匿名对象，像Grouping，FromQuery中的参数访问的实体类成员
    /// </summary>
    AnonymousObject = 3,
    /// <summary>
    /// 访问了实体的IncludeMany的成员，当前字段是主表主键字段
    /// </summary>
    MasterField = 4
}