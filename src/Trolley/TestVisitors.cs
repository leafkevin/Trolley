using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class DeferredExpressionVisitor : ExpressionVisitor
{
    private readonly SqlVisitor sqlVisitor;
    private bool isVisited;
    private bool hasMemberAccess;
    private readonly List<ParameterExpression> fieldsParameters = new();
    private readonly List<ParameterExpression> valuesParameters = new();
    private List<ReaderField> readerFields = null;
    private List<object> localValues = new();

    public DeferredExpressionVisitor(SqlVisitor sqlVisitor)
    {
        this.sqlVisitor = sqlVisitor;
    }
    public ReaderField Build(Expression expr)
    {
        var rawSql = "NULL";
        if (this.readerFields != null && this.readerFields.Count > 0)
        {
            var builder = new StringBuilder();
            foreach (var readerField in this.readerFields)
            {
                if (builder.Length > 0)
                    builder.Append(',');
                builder.Append(readerField.Value.ToString());
            }
            rawSql = builder.ToString();
        }
        return new ReaderField
        {
            IsDeferredFields = true,
            FieldType = ReaderFieldType.Field,
            Expression = this.Visit(expr),
            Fields = this.readerFields,
            FieldParameters = this.fieldsParameters,
            ValuesParameters = this.valuesParameters,
            LocalValues = this.localValues,
            Value = rawSql
        };
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression?.NodeType == ExpressionType.Parameter)
        {
            this.hasMemberAccess = true;
            if (this.isVisited) return base.VisitMember(node);

            var sqlSegment = this.sqlVisitor.Visit(new SqlSegment { Expression = node });
            this.readerFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                ReaderType = node.Type,
                MemberMapper = sqlSegment.MemberMapper,
                MemberName = sqlSegment.MemberName,
                //TypeHandler = sqlSegment.TypeHandler,
                Value = sqlSegment.Value
            });
            var parameterExpr = node.Expression as ParameterExpression;
            var parameterName = $"{parameterExpr.Name}${node.Member.Name}";

            parameterExpr = this.fieldsParameters.Find(f => f.Name == parameterName);
            if (parameterExpr != null) return parameterExpr;
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.fieldsParameters.Add(parameterExpr);
            return parameterExpr;
        }

        if (TryGetClosureValue(node, out var closureValue))
        {
            var parameterName = $"lv{this.localValues.Count}";
            var parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.valuesParameters.Add(parameterExpr);
            this.localValues.Add(closureValue);
            return parameterExpr;
        }
        return base.VisitMember(node);
    }
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        this.hasMemberAccess = false;
        var isSupportMethod = node.Method.DeclaringType == typeof(Sql) ||
            typeof(IAggregateSelect).IsAssignableFrom(node.Method.DeclaringType);

        var result = base.VisitMethodCall(node);
        //支持IsNull、Max、Min、Sum、Avg等聚合函数的参数表达式中包含成员访问表达式的情况
        if (isSupportMethod && this.hasMemberAccess)
        {
            var sqlSegment = this.sqlVisitor.Visit(new SqlSegment { Expression = node });
            if (sqlSegment.SqlType == SqlType.ReaderField)
                this.readerFields.Add(sqlSegment.Value as ReaderField);
            else
            {
                var rawSql = this.sqlVisitor.WrapSql(sqlSegment);
                this.readerFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.RawSql,
                    ReaderType = node.Type,
                    Value = rawSql
                });
            }

            var parameterName = $"{node.Method.Name}${this.fieldsParameters.Count}";
            var parameterExpr = this.fieldsParameters.Find(f => f.Name == parameterName);
            if (parameterExpr != null) return parameterExpr;
            parameterExpr = Expression.Parameter(node.Type, parameterName);
            this.fieldsParameters.Add(parameterExpr);

            this.isVisited = true;
            return parameterExpr;
        }
        return result;
    }
    private static bool TryGetClosureValue(MemberExpression node, out object value)
    {
        if (node.Expression is ConstantExpression constantExpr)
        {
            value = ValueEvalutor.Evaluate(node.Member, constantExpr.Value);
            return true;
        }
        else if (node.Expression is MemberExpression memberExpr && TryGetClosureValue(memberExpr, out var parentValue))
        {
            value = ValueEvalutor.Evaluate(node.Member, parentValue);
            return true;
        }
        value = null;
        return false;
    }
}
public class HasParameterVisitor : ExpressionVisitor
{
    private bool hasMemberAccess;
    public bool HasParameter { get; private set; }
    public bool HasVariable => this.hasMemberAccess && !this.HasParameter;
    public List<ParameterExpression> Parameters { get; private set; }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        this.HasParameter = true;
        this.Parameters ??= new();
        if (!this.Parameters.Contains(node))
            this.Parameters.Add(node);
        return node;
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression.NodeType == ExpressionType.Constant)
            this.hasMemberAccess = true;
        return base.VisitMember(node);
    }
}