using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Trolley;

public class TheaDbParameterCollection : List<IDbDataParameter>, IDataParameterCollection
{
    private readonly Dictionary<string, int> namedIndices = new();
    public object this[string parameterName]
    {
        get
        {
            if (this.namedIndices.TryGetValue(parameterName, out var index))
                return base[index];
            return null;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is not IDbDataParameter dbParameter)
                throw new Exception("只支持IDbDataParameter类型参数");

            if (this.namedIndices.TryGetValue(parameterName, out var index))
                base[index] = dbParameter;
        }
    }
    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public int Add(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");
        if (this.namedIndices.ContainsKey(dbParameter.ParameterName))
            throw new Exception($"参数{dbParameter.ParameterName}已存在");

        var index = this.Count;
        this.namedIndices.Add(dbParameter.ParameterName, index);
        base.Add(dbParameter);
        return index;
    }
    public bool Contains(string parameterName)
        => this.namedIndices.ContainsKey(parameterName);
    public bool Contains(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");
        return this.namedIndices.ContainsKey(dbParameter.ParameterName);
    }
    public void CopyTo(Array array, int index)
        => ((ICollection)this).CopyTo(array, index);
    public int IndexOf(string parameterName)
    {
        if (this.namedIndices.TryGetValue(parameterName, out var index))
            return index;
        return -1;
    }
    public int IndexOf(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");
        return this.IndexOf(dbParameter.ParameterName);
    }
    public void Insert(int index, object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");
        if (index >= this.Count || index < 0)
            throw new NotSupportedException($"index索引已经超过参数集合范围，当前Count:{this.Count},index:{index}");
        if (this.namedIndices.ContainsKey(dbParameter.ParameterName))
            throw new Exception($"参数{dbParameter.ParameterName}已存在");

        for (int i = index; i < this.Count; i++)
            this.namedIndices[base[i].ParameterName] = i + 1;
        this.namedIndices.Add(dbParameter.ParameterName, index);
        base.Insert(index, dbParameter);
    }
    public void Remove(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");
        this.RemoveAt(dbParameter.ParameterName);
    }
    public void RemoveAt(string parameterName)
    {
        if (this.namedIndices.Remove(parameterName, out var index))
            base.RemoveAt(index);
    }
}
