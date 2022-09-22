using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 针对当前操作数的延时表达式处理
/// </summary>
public class DeferredExpr
{
    public ExpressionType ExpressionType { get; set; }
    /// <summary>
    /// SqlSegment.Null/SqlSegment.True/null常量或是字符串连接操作的List<Expression>或是Expression成员访问表达式
    /// </summary>
    public object Value { get; set; }
}