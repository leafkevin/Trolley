using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Trolley;

public class TheaDbParameterCollection : IDataParameterCollection
{
    private readonly List<IDbDataParameter> parameters = new();
    private readonly Dictionary<string, int> namedIndices = new();
    public object this[string parameterName]
    {
        get
        {
            if (this.namedIndices.TryGetValue(parameterName, out var index))
                return this.parameters[index];
            return null;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is not IDbDataParameter dbParameter)
                throw new Exception("只支持IDbDataParameter类型参数");

            if (!this.namedIndices.TryGetValue(parameterName, out var index))
                this.namedIndices.TryAdd(parameterName, index = this.parameters.Count);
            this.parameters[index] = dbParameter;
        }
    }

    public object this[int index]
    {
        get
        {
            if (index < 0 || index >= this.parameters.Count)
                throw new IndexOutOfRangeException($"index索引已经超过参数集合范围，当前Count:{this.Count},index:{index}");
            return this.parameters[index];
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value is not IDbDataParameter dbParameter)
                throw new Exception("只支持IDbDataParameter类型参数");
            this.parameters[index] = dbParameter;
        }
    }

    public bool IsFixedSize => false;
    public bool IsReadOnly => false;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public int Count => this.parameters.Count;
    public int Add(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (value is not IDbDataParameter dbParameter)
            throw new Exception("只支持IDbDataParameter类型参数");

        var index = this.Count;
        if (!this.namedIndices.TryAdd(dbParameter.ParameterName, index))
            throw new Exception($"参数{dbParameter.ParameterName}已存在，请考虑使用ToParameter方法，更改子查询或是CTE子句中的参数名，避免参数名重复，如：.Where(f => f.Id > orderId.ToParameter(\"@OrderId\"))");
        this.parameters.Add(dbParameter);
        return index;
    }
    public void Clear()
    {
        this.namedIndices.Clear();
        this.parameters.Clear();
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
    public IEnumerator GetEnumerator() => this.parameters.GetEnumerator();
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
        if (index < 0 || index >= this.parameters.Count)
            throw new IndexOutOfRangeException($"index索引已经超过参数集合范围，当前Count:{this.Count},index:{index}");
        if (this.namedIndices.ContainsKey(dbParameter.ParameterName))
            throw new Exception($"参数{dbParameter.ParameterName}已存在");

        for (int i = index; i < this.Count; i++)
            this.namedIndices[this.parameters[i].ParameterName] = i + 1;
        this.namedIndices.Add(dbParameter.ParameterName, index);
        this.parameters.Insert(index, dbParameter);
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
            this.RemoveAt(index);
    }
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= this.parameters.Count)
            throw new IndexOutOfRangeException($"index索引已经超过参数集合范围，当前Count:{this.Count},index:{index}");
        var parameterName = this.parameters[index].ParameterName;
        this.namedIndices.Remove(parameterName);
        this.parameters.RemoveAt(index);
    }
    public List<IDbDataParameter> ToList() => this.parameters;
}
