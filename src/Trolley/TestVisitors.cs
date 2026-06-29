using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class DeferredExpressionVisitor : ExpressionVisitor
{
    private readonly SqlVisitor sqlVisitor;
    private bool isAggSelect;
    private bool hasMemberAccess;
    private List<ParameterExpression> parameters = new();
    private List<ReaderField> readerFields = new();

    public DeferredExpressionVisitor(SqlVisitor sqlVisitor)
    {
        this.sqlVisitor = sqlVisitor;
    }
    public (string, List<ReaderField>, List<ParameterExpression>) BuildSql()
    {
        if (this.parameters.Count == 0)
            return ("NULL", null, null);

        var builder = new StringBuilder();
        foreach (var readerField in readerFields)
        {
            if (builder.Length > 0)
                builder.Append(',');
            builder.Append(readerField.Value.ToString());
        }
        var sql = builder.ToString();
        return (sql, this.readerFields, this.parameters);
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression.NodeType == ExpressionType.Parameter)
        {
            this.hasMemberAccess = true;
            if (this.isAggSelect) return base.VisitMember(node);

            var sqlSegment = this.sqlVisitor.Visit(new SqlSegment { Expression = node });
            this.readerFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                ReaderType = node.Type,
                TypeHandler = sqlSegment.TypeHandler,
                Value = sqlSegment.Value
            });

            var parameterExpr = node.Expression as ParameterExpression;
            var parameterName = $"{parameterExpr.Name}${node.Member.Name}";
            parameterExpr = this.parameters.Find(f => f.Name == parameterName);
            if (parameterExpr != null) return parameterExpr;
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.parameters.Add(parameterExpr);
            return parameterExpr;
        }
        return base.VisitMember(node);
    }
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        this.hasMemberAccess = false;
        this.isAggSelect = typeof(IAggregateSelect).IsAssignableFrom(node.Method.DeclaringType);
        var result = base.VisitMethodCall(node);
        if (this.isAggSelect && this.hasMemberAccess)
        {
            var sqlSegment = this.sqlVisitor.Visit(new SqlSegment { Expression = node });
            var rawSql = this.sqlVisitor.WrapSql(sqlSegment);
            this.readerFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.RawSql,
                ReaderType = node.Type,
                Value = rawSql
            });

            var parameterName = $"{node.Method.Name}${this.parameters.Count}";
            var parameterExpr = this.parameters.Find(f => f.Name == parameterName);
            if (parameterExpr != null) return parameterExpr;
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.parameters.Add(parameterExpr);
            return parameterExpr;
        }
        return result;
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