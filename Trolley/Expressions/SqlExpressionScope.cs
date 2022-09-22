using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public enum SqlSegmentType : byte
{
    None = 0,
    From,
    InnerJoin,
    LeftJoin,
    RightJoin,
    Select,
    Include,
    Distinct,
    Where,
    Take,
    Skip,
    Paging,
    OrderBy,
    OrderByDescending,
    //ThenBy,
    //ThenByDesc,
    GroupBy,
    //Include,
    //Aggregate 
}

class SqlExpressionScope
{
    /// <summary>
    /// Expression,SqlExpressionScope
    /// </summary>
    public object Value { get; set; }
    public string Separator { get; set; }
    public Expression Source { get; set; }
    /// <summary>
    /// Expression,SqlExpressionScope
    /// </summary>
    public Stack<object> NextExprs { get; set; }
    public SqlExpressionScope Parent { get; set; }
    public int Deep { get; set; }

    public SqlExpressionScope() { }
    public SqlExpressionScope(string separator) => this.Separator = separator;
    public void Push(object scopeExpr)
    {
        if (this.NextExprs == null)
            this.NextExprs = new Stack<object>();
        this.NextExprs.Push(scopeExpr);
        if (scopeExpr is SqlExpressionScope currentScope)
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