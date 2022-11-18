using System.Linq.Expressions;

namespace ConsoleAppTest;

class TestVisitor : ExpressionVisitor
{
    public bool IsParameter { get; private set; }
    public bool IsConstant { get; private set; }
    public bool IsBinary { get; private set; }
    public bool IsMethodCall { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (!this.IsParameter) this.IsParameter = true;
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