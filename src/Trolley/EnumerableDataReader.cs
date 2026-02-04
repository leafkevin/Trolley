using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Trolley;

public class EnumerableDataReader : IDataReader
{
    private readonly IEnumerator enumerator;
    private readonly List<MemberMap> memberMappers;
    private readonly List<Func<object, object>> valueGetters;
    private readonly Dictionary<string, int> nameOrdinals;
    private object current;
    private bool isClosed;

    public EnumerableDataReader(IEnumerable collection, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters)
    {
        this.enumerator = collection.GetEnumerator();
        this.memberMappers = memberMappers;
        this.valueGetters = valueGetters;
        this.nameOrdinals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < memberMappers.Count; i++)
        {
            this.nameOrdinals[memberMappers[i].FieldName] = i;
        }
    }

    public object this[int i] => this.GetValue(i);
    public object this[string name] => this.GetValue(this.GetOrdinal(name));
    public int Depth => 0;
    public bool IsClosed => this.isClosed;
    public int RecordsAffected => -1;
    public int FieldCount => this.memberMappers.Count;

    public void Close()
    {
        this.isClosed = true;
        (this.enumerator as IDisposable)?.Dispose();
    }

    public void Dispose() => this.Close();

    public bool Read()
    {
        if (this.enumerator.MoveNext())
        {
            this.current = this.enumerator.Current;
            return true;
        }
        return false;
    }

    public object GetValue(int i)
    {
        var getter = this.valueGetters[i];
        var value = getter(this.current);
        // MySqlConnector expects DBNull for null values, not C# null
        return value ?? DBNull.Value; 
    }

    public string GetName(int i) => this.memberMappers[i].FieldName;
    public int GetOrdinal(string name) => this.nameOrdinals.TryGetValue(name, out var ordinal) ? ordinal : -1;
    public string GetDataTypeName(int i) => this.memberMappers[i].NativeDbType.ToString();
    public Type GetFieldType(int i) => this.memberMappers[i].MemberType;
    public DataTable GetSchemaTable() => throw new NotSupportedException();
    
    public bool GetBoolean(int i) => (bool)this.GetValue(i);
    public byte GetByte(int i) => (byte)this.GetValue(i);
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
    public char GetChar(int i) => (char)this.GetValue(i);
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => throw new NotSupportedException();
    public IDataReader GetData(int i) => throw new NotSupportedException();
    public DateTime GetDateTime(int i) => (DateTime)this.GetValue(i);
    public decimal GetDecimal(int i) => (decimal)this.GetValue(i);
    public double GetDouble(int i) => (double)this.GetValue(i);
    public float GetFloat(int i) => (float)this.GetValue(i);
    public Guid GetGuid(int i) => (Guid)this.GetValue(i);
    public short GetInt16(int i) => (short)this.GetValue(i);
    public int GetInt32(int i) => (int)this.GetValue(i);
    public long GetInt64(int i) => (long)this.GetValue(i);
    public string GetString(int i) => (string)this.GetValue(i);
    public int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, this.FieldCount);
        for (int i = 0; i < count; i++)
        {
            values[i] = this.GetValue(i);
        }
        return count;
    }
    public bool IsDBNull(int i) => this.GetValue(i) is DBNull;
    public bool NextResult() => false;
}