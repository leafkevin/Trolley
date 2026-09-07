using System.Text;

namespace Trolley;

public class WhereSqlBuilder
{
    private int startIndex;
    private OperationType operationType { get; set; }

    public OperationType LastWhereOperationType { get; set; } = OperationType.None;
    public StringBuilder WhereBuilder { get; set; } = new();

    public WhereSqlBuilder()
    {
        this.WhereBuilder = new();
    }

    public virtual void AndSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var lastOperationType = this.WhereBuilder.Length > 0 ? OperationType.And : operationType;
        if (this.LastWhereOperationType == OperationType.Or)
        {
            this.WhereBuilder.Insert(0, '(');
            this.WhereBuilder.Append(')');
        }
        if (this.WhereBuilder.Length > 0)
        {
            this.WhereBuilder.Append(" AND ");
            if (operationType == OperationType.Or)
                whereSql = $"({whereSql})";
        }
        this.WhereBuilder.Append(whereSql);
        this.LastWhereOperationType = lastOperationType;
    }
    public virtual void OrSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var lastOperationType = this.WhereBuilder.Length > 0 ? OperationType.Or : operationType;
        if (this.WhereBuilder.Length > 0)
            this.WhereBuilder.Append(" OR ");
        this.WhereBuilder.Append(whereSql);
        this.LastWhereOperationType = lastOperationType;
    }
    public string Build() => this.WhereBuilder.ToString();
    public void AsRefQueryObj()
    {
        this.startIndex = this.WhereBuilder.Length;
        this.operationType = this.LastWhereOperationType;
    }
    public void Clear()
    {
        this.LastWhereOperationType = OperationType.None;
        this.WhereBuilder.Clear();
    }
}