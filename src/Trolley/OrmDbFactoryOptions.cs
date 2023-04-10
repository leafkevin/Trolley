namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int Timeout { get; set; } = 30;
    /// <summary>
    /// 是否使用参数化，此为全局设置，如果设置为true，所有的查询语句中用到的变量都将变成参数
    /// Create、Update、Delete操作本身就是参数化的，主要是Lambda表达式中用到的变量，如：
    /// string productNo="xxx";//变量，会使用参数化
    /// using var repository = dbFactory.Create().WithParameterized();
    /// var result1 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains(productNo));
    /// var result2 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains("PN-001"));//常量，不会使用参数化
    /// </summary>
    public bool IsNeedParameter { get; set; }
}
