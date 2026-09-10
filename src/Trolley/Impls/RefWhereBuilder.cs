using System;
using System.Data;
using System.Text;

namespace Trolley;

public class RefWhereBuilder : IDisposable, ICloneable
{
    private bool hasSavePoint;
    private int savedParametersIndex;
    private int savedWhereIndex;
    private OperationType savedOperationType;

    private OperationType current = OperationType.None;
    private StringBuilder whereBuilder = new();
    private IDataParameterCollection dbParameters;
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
    public RefWhereBuilder Clone()
    {
        var clone = new RefWhereBuilder();
        clone.savedWhereIndex = this.savedWhereIndex;
        clone.savedOperationType = this.savedOperationType;
        clone.current = this.current;
        clone.whereBuilder = new StringBuilder(this.whereBuilder.ToString());
        return clone;
    }
    public void Save(IDataParameterCollection dbParameters)
    {
        this.hasSavePoint = true;
        this.savedWhereIndex = this.whereBuilder.Length;
        this.savedOperationType = this.current;
        this.dbParameters = dbParameters;
        this.savedParametersIndex = dbParameters.Count;
    }
    public void Release()
    {
        if (!this.hasSavePoint) return;
        if (this.savedWhereIndex > 0 && this.whereBuilder.Length > this.savedWhereIndex)
        {
            this.current = this.savedOperationType;
            var length = this.whereBuilder.Length - this.savedWhereIndex;
            this.whereBuilder.Remove(savedWhereIndex, length);
        }
        if (this.savedParametersIndex > 0)
        {
            while (this.dbParameters.Count > this.savedParametersIndex)
                this.dbParameters.RemoveAt(this.savedParametersIndex);
        }
    }
    public override string ToString() => this.Build();
    object ICloneable.Clone() => this.Clone();
    public void Dispose()
    {
        this.whereBuilder.Clear();
        this.whereBuilder = null;
        this.dbParameters = null;
    }
}