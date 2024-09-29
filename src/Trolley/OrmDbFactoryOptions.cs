using System;

namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int Timeout { get; set; } = 30;
    /// <summary>
    /// 表达式解析中，所有变量都会参数化，常量不会参数化。如果设置为true，所有常量也将都会参数化
    /// </summary>
    public bool IsParameterized { get; set; }
    /// <summary>
    /// 枚举类型常量或变量映射到数据库的默认类型，默认值是int类型
    /// </summary>
    public Type DefaultEnumMapDbType { get; set; } = typeof(int);
}