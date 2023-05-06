namespace Trolley;

public enum OperationType
{
    None = 0,
    Equal,
    Not,
    And,
    Or
}
/// <summary>
/// 针对当前操作数的延时表达式处理
/// </summary>
public struct DeferredExpr
{
    /// <summary>
    /// 操作符：And/Or/Equal/Not
    /// </summary>
    public OperationType OperationType { get; set; }
    /// <summary>
    /// SqlSegment.Null/SqlSegment.True/null常量或是字符串连接操作的List<Expression>或是Expression成员访问表达式
    /// </summary>
    public object Value { get; set; }
}