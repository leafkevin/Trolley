namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int Timeout { get; set; } = 30;
    /// <summary>
    /// 所有表达式解析中用到的变量默认参数，如果设置为true，所有表达式解析中用到的常量也将都变成参数，如：
    /// string productNo="xxx";//变量
    /// await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains(productNo));//变量，默认使用参数化
    /// await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains("PN-001"));//常量，设置为true，将使用参数化
    /// </summary>
    public bool IsParameterized { get; set; }
}
