using System;
using System.Text;

namespace Trolley;

public class ReusableWhereBuilder : IDisposable
{
    private bool hasSavePoint;
    private string whereSql;
    private OperationType savedOperationType;

    private OperationType current = OperationType.None;
    private StringBuilder whereBuilder = new();
    public bool HasSql => this.whereBuilder?.Length > 0;

    public virtual void AndSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var currentType = this.whereBuilder.Length > 0 ? OperationType.And : operationType;
        if (this.current == OperationType.Or)
        {
            this.whereBuilder.Insert(0, '(');
            this.whereBuilder.Append(')');
        }
        if (this.whereBuilder.Length > 0)
        {
            this.whereBuilder.Append(" AND ");
            if (operationType == OperationType.Or)
                whereSql = $"({whereSql})";
        }
        this.whereBuilder.Append(whereSql);
        this.current = currentType;
    }
    public virtual void OrSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var currentType = this.whereBuilder.Length > 0 ? OperationType.Or : operationType;
        if (this.whereBuilder.Length > 0)
            this.whereBuilder.Append(" OR ");
        this.whereBuilder.Append(whereSql);
        this.current = currentType;
    }
    public string Build() => this.whereBuilder.ToString();
    public void Clear()
    {
        this.current = OperationType.None;
        this.whereBuilder.Clear();
    }
    public void CloneTo(ReusableWhereBuilder target)
    {
        var sql = this.whereBuilder.ToString();
        target.hasSavePoint = this.hasSavePoint;
        target.whereSql = sql;
        target.savedOperationType = this.savedOperationType;
        target.current = this.current;
        target.whereBuilder = new StringBuilder(sql);
    }
    public void Save()
    {
        this.whereSql = this.whereBuilder.ToString();
        this.savedOperationType = this.current;
        this.hasSavePoint = true;
    }
    public void Reset()
    {
        if (!this.hasSavePoint) return;
        this.current = this.savedOperationType;
        this.whereBuilder.Clear();
        this.whereBuilder.Append(this.whereSql);
    }
    public override string ToString() => this.Build();
    public void Dispose()
    {
        this.whereBuilder.Clear();
        this.whereBuilder = null;
    }
}