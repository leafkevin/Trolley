using System;
using System.Data;
using System.Text;

namespace Trolley;

public class RefWhereBuilder : IDisposable, ICloneable
{
    private bool hasSavePoint;
    private int whereIndex;
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
    public RefWhereBuilder Clone() => new RefWhereBuilder
    {
        whereIndex = this.whereIndex,
        savedOperationType = this.savedOperationType,
        current = this.current,
        whereBuilder = new StringBuilder(this.whereBuilder.ToString())
    };
    public void Save()
    {
        if (this.whereBuilder.Length == 0) return;
        this.whereIndex = this.whereBuilder.Length;
        this.savedOperationType = this.current;
        this.hasSavePoint = true;
    }
    public void Release()
    {
        if (!this.hasSavePoint) return;
        if (this.whereIndex < 0 || this.whereBuilder.Length <= this.whereIndex)
            return;
        this.current = this.savedOperationType;
        var length = this.whereBuilder.Length - this.whereIndex;
        this.whereBuilder.Remove(whereIndex, length);
    }
    public override string ToString() => this.Build();
    object ICloneable.Clone() => this.Clone();
    public void Dispose()
    {
        this.whereBuilder.Clear();
        this.whereBuilder = null;
    }
}