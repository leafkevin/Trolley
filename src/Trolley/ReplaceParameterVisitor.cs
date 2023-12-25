using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

class ReplaceParameterVisitor : ExpressionVisitor
{
    public List<ParameterExpression> NewParameters { get; private set; } = new();
    public List<MemberExpression> OrgMembers { get; private set; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression.NodeType == ExpressionType.Parameter)
        {
            var parameterName = $"args{this.NewParameters.Count}";
            ParameterExpression parameterExpr = null;
            if (this.NewParameters != null)
            {
                parameterExpr = NewParameters.Find(f => f.Name == parameterName);
                if (parameterExpr != null) return parameterExpr;
            }

            this.OrgMembers.Add(node);
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.NewParameters.Add(parameterExpr);
            return parameterExpr;
        }
        return base.VisitMember(node);
    }
}