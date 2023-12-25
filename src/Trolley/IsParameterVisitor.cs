using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

class IsParameterVisitor : ExpressionVisitor
{
    public bool IsParameter { get; private set; }
    public string LastParameterName { get; private set; }
    public List<ParameterExpression> Parameters { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.IsParameter = true;
        this.LastParameterName = node.Name;
        this.Parameters ??= new();
        if (!this.Parameters.Contains(node))
            this.Parameters.Add(node);
        return node;
    }
}