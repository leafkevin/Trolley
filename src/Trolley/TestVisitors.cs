using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class MemberVisitor : ExpressionVisitor
{
    public List<Expression> Members { get; private set; } = new();

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
public class ReplaceParameterVisitor : ExpressionVisitor
{
    private bool isChanged = false;
    private Expression expression;
    private Expression parameterExpr;
    private bool hasParameter;

    public string MemberName { get; set; }

    public bool HasParameter(Expression expression)
    {
        this.expression = expression;
        this.Visit(expression);
        return this.hasParameter;
    }
    public Expression Change(Expression parameterExpr)
    {
        this.isChanged = true;
        this.parameterExpr = parameterExpr;
        return this.Visit(this.expression);
    }
    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.hasParameter = true;
        if (this.isChanged) return this.parameterExpr;
        return base.VisitParameter(node);
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        this.MemberName = node.Member.Name;
        return base.VisitMember(node);
    }
}
public class HasParameterVisitor : ExpressionVisitor
{
    public bool HasParameter { get; private set; }
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
}