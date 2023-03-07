using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

class TestVisitor : ExpressionVisitor
{
    public bool IsParameter { get; private set; }
    public string LastParameterName { get; private set; }
    public List<ParameterExpression> Parameters { get; private set; }
    public bool IsConstant { get; private set; }
    public bool IsBinary { get; private set; }
    public bool IsMethodCall { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.IsParameter = true;
        this.LastParameterName = node.Name;
        this.Parameters ??= new();
        if (!this.Parameters.Contains(node))
            this.Parameters.Add(node);
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