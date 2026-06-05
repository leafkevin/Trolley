using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MemberVisitor : ExpressionVisitor
{
    public List<MemberExpression> Members { get; private set; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression.NodeType == ExpressionType.Parameter)
            this.Members.Add(node);
        return base.VisitMember(node);
    }
}
public class ReplaceMemberVisitor : ExpressionVisitor
{
    public List<ParameterExpression> NewParameters { get; private set; } = new();
    public List<MemberExpression> OrgMembers { get; private set; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression.NodeType == ExpressionType.Parameter)
        {
            var parameterExpr = node.Expression as ParameterExpression;
            var parameterName = $"{parameterExpr.Name}${node.Member.Name}";
            parameterExpr = NewParameters.Find(f => f.Name == parameterName);
            if (parameterExpr != null) return parameterExpr;
            this.OrgMembers.Add(node);
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.NewParameters.Add(parameterExpr);
            return parameterExpr;
        }
        return base.VisitMember(node);
    }
}
public class HasParameterVisitor : ExpressionVisitor
{
    private bool hasMemberAccess;
    public bool HasParameter { get; private set; }
    public bool HasVariable => this.hasMemberAccess && !this.HasParameter;
    public string LastParameterName { get; private set; }
    public List<ParameterExpression> Parameters { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.HasParameter = true;
        this.LastParameterName = node.Name;
        this.Parameters ??= new();
        if (!this.Parameters.Contains(node))
            this.Parameters.Add(node);
        return node;
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression == null)
            this.hasMemberAccess = true;
        return base.VisitMember(node);
    }
}