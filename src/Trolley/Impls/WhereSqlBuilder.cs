using System;
using System.Data;
using System.Text;

namespace Trolley;

public class WhereSqlBuilder : IDisposable, ICloneable
{
    private bool isRefQueryObj;
    private bool isInitialized;
    protected int dbParametersIndex;
    private int whereIndex;
    private OperationType operationType;
    private OperationType lastOperationType = OperationType.None;
    private StringBuilder whereBuilder = new();
    private IDataParameterCollection dbParameters;
    public bool HasSql => this.whereBuilder?.Length > 0;

    public virtual void AndSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var lastOperationType = this.whereBuilder.Length > 0 ? OperationType.And : operationType;
        if (this.lastOperationType == OperationType.Or)
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
        this.lastOperationType = lastOperationType;
    }
    public virtual void OrSql(string whereSql, OperationType operationType = OperationType.None)
    {
        var lastOperationType = this.whereBuilder.Length > 0 ? OperationType.Or : operationType;
        if (this.whereBuilder.Length > 0)
            this.whereBuilder.Append(" OR ");
        this.whereBuilder.Append(whereSql);
        this.lastOperationType = lastOperationType;
    }
    public string Build()
    {
        this.isInitialized = false;
        return this.whereBuilder.ToString();
    }
    public void AsRefQueryObj(IDataParameterCollection dbParameters)
    {
        this.whereIndex = this.whereBuilder.Length;
        this.operationType = this.lastOperationType;
        this.dbParameters = dbParameters;
        this.dbParametersIndex = dbParameters.Count;
        this.isRefQueryObj = true;
    }
    public void Clear()
    {
        this.lastOperationType = OperationType.None;
        this.whereBuilder.Clear();
        if (this.dbParametersIndex > 0)
        {
            while (this.dbParameters.Count > this.dbParametersIndex)
                this.dbParameters.RemoveAt(this.dbParametersIndex);
        }
    }
    public WhereSqlBuilder Clone()
    {
        var clone = new WhereSqlBuilder();
        clone.whereIndex = this.whereIndex;
        clone.operationType = this.operationType;
        clone.lastOperationType = this.lastOperationType;
        clone.whereBuilder = new StringBuilder(this.whereBuilder.ToString());
        return clone;
    }
    public void Dirty()
    {
        if (!this.isRefQueryObj) return;
        this.isInitialized = false;
        if (this.whereIndex > 0 && this.whereBuilder.Length > this.whereIndex)
        {
            this.lastOperationType = this.operationType;
            var length = this.whereBuilder.Length - this.whereIndex;
            this.whereBuilder.Remove(whereIndex, length);
        }
        if (this.dbParametersIndex > 0)
        {
            while (this.dbParameters.Count > this.dbParametersIndex)
                this.dbParameters.RemoveAt(this.dbParametersIndex);
        }
    }
    public override string ToString() => this.Build();
    object ICloneable.Clone() => this.Clone();
    public void Dispose()
    {
        this.whereBuilder.Clear();
        this.whereBuilder = null;
    }
}