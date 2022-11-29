using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

class TestVisitor : ExpressionVisitor
{
    public bool IsParameter { get; private set; }
    public string LastParameterName { get; private set; }
    public List<string> ParameterNames { get; private set; } = new List<string>();
    public bool IsConstant { get; private set; }
    public bool IsBinary { get; private set; }
    public bool IsMethodCall { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.IsParameter = true;
        this.LastParameterName = node.Name;
        if (!this.ParameterNames.Contains(node.Name))
            this.ParameterNames.Add(node.Name);
        return node;
    }
    protected override Expression VisitConstant(ConstantExpression node)
    {
        this.IsConstant = true;
        return base.VisitConstant(node);
    }
    protected override Expression VisitBinary(BinaryExpression node)
    {
        this.IsBinary = true;
        return base.VisitBinary(node);
    }
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        this.IsMethodCall = true;
        return base.VisitMethodCall(node);
    }
}