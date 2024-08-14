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
}
