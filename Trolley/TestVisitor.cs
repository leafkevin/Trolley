using System.Linq.Expressions;

namespace Trolley;

class TestVisitor : ExpressionVisitor
{
    public bool IsParameter { get; private set; }
    public string ParameterName { get; private set; }
    public bool IsConstant { get; private set; }
    public bool IsBinary { get; private set; }
    public bool IsMethodCall { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.IsParameter = true;
        this.ParameterName = node.Name;
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