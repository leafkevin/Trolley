using System;
using System.Data;

namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int Timeout { get; set; } = 30;
    /// <summary>
    /// PN-001所有表达式中的常量，如果设置为true，所有表达式解析中用到的常量也将都变成参数，如：
    /// <code>var result = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains("PN-001"));</code>//常量PN-001，会被参数化
    /// 默认情况下，所有变量都会参数化处理
    /// </summary>
    public bool IsParameterized { get; set; }
    public Action<IDbCommand> ExecuteBeforeFilter { get; set; }
    public Action<IDbCommand> ExecuteAfterFilter { get; set; }
    public Action<IDbCommand, object> InsertFilter { get; set; }
    public Action<IDbCommand, object> UpdateFilter { get; set; }
    public Action<IDbCommand, Exception> ExceptionFilter { get; set; }
}
