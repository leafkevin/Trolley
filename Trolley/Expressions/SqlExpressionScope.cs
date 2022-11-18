using System.Collections.Generic;

namespace Trolley;

public enum SqlSegmentType : byte
{
    None = 0,
    /// <summary>
    /// MemberVisit(有运算),Const,Func<bool>
    /// </summary>
    Where,
    /// <summary>
    /// MemberVisit(有运算),New(无Member)
    /// </summary>
    GroupBy,
    /// <summary>
    /// MemberVisit(有运算),New(有Member),MemberInit,IQuery,Const
    /// </summary>
    Select,
    /// <summary>
    /// MemberVisit(无运算)
    /// </summary>
    Include,
    Tracking
}

class WhereScope
{
    /// <summary>
    /// Expression,WhereScope
    /// </summary>
    public object Value { get; set; }
    /// <summary>
    /// 解析where子句中使用的AND/OR
    /// </summary>
    public string Separator { get; set; }
    /// <summary>
    /// Expression,WhereScope
    /// </summary>
    public Stack<object> NextExprs { get; set; }
    public WhereScope Parent { get; set; }
    public int Deep { get; set; }
    public WhereScope(string separator) => this.Separator = separator;
    public void Push(object scopeExpr)
    {
        this.NextExprs ??= new Stack<object>();
        this.NextExprs.Push(scopeExpr);
        if (scopeExpr is WhereScope currentScope)
        {
            currentScope.Deep = this.Deep + 1;
            currentScope.Parent = this;
        }
    }
    public bool TryPop(out object scopeExpr)
    {
        if (this.NextExprs == null || this.NextExprs.Count == 0)
        {
            scopeExpr = null;
            return false;
        }
        return this.NextExprs.TryPop(out scopeExpr);
    }
}