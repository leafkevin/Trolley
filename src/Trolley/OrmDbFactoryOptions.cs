namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int Timeout { get; set; } = 30;
    /// <summary>
    /// 是否需要参数化，如果设置为true，所有的查询语句中用到的常量、变量都将变成参数
    /// Create、Update、Delete操作本身就是参数化的，主要是Lambda表达式中用到的常量、变量
    /// </summary>
    /// <param name="isRequired">必须使用参数化</param>
    /// <returns>返回仓储对象</returns>
    public bool IsNeedParameter { get; set; }
}
