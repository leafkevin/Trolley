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
public class ReplaceParameterVisitor : ExpressionVisitor
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