using MySqlConnector;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaDataReader : ITheaDataReader
{
    private readonly MySqlDataReader reader;   

    public int Depth => this.reader.Depth;
    public bool IsClosed => this.reader.IsClosed;
    public int RecordsAffected => this.reader.RecordsAffected;
    public int FieldCount => this.reader.FieldCount;
    public object this[string name] => this.reader[name];
    public object this[int i] => this.reader[i];

    public IDataReader DbDataReader => this.reader;

    public MySqlTheaDataReader(MySqlDataReader reader) => this.reader = reader;

    public void Close() => this.reader.Close();
    public Task CloseAsync()
    {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return this.reader.CloseAsync();
#else
        this.reader.Close();
        return Task.CompletedTask;
#endif
    }
    public void Dispose() => this.reader.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public ValueTask DisposeAsync()
    {
        this.reader.DisposeAsync();
        return default(ValueTask);
    }
#else
    public ValueTask DisposeAsync()
    {
        this.reader.Dispose();
        return default(ValueTask);
    }
#endif
    public T GetFieldValue<T>(int ordinal) => this.reader.GetFieldValue<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal)
        => this.reader.GetFieldValueAsync<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        => this.reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
    public bool IsDBNull(int i) => this.reader.IsDBNull(i);
    public Task<bool> IsDBNullAsync(int ordinal) => this.reader.IsDBNullAsync(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        => this.reader.IsDBNullAsync(ordinal, cancellationToken);
    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken = default)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        => this.reader.ReadAsync(cancellationToken);

    public DataTable GetSchemaTable() => this.reader.GetSchemaTable();
 
    public bool GetBoolean(int i) => this.reader.GetBoolean(i);
    public byte GetByte(int i) => this.reader.GetByte(i);
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        => this.reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    public char GetChar(int i) => this.reader.GetChar(i);
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        => this.reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
    public IDataReader GetData(int i) => this.reader.GetData(i);
    public string GetDataTypeName(int i) => this.reader.GetDataTypeName(i);
    public DateTime GetDateTime(int i) => this.reader.GetDateTime(i);
    public decimal GetDecimal(int i) => this.reader.GetDecimal(i);
    public double GetDouble(int i) => this.reader.GetFloat(i);
    public Type GetFieldType(int i) => this.reader.GetFieldType(i);
    public float GetFloat(int i) => this.reader.GetFloat(i);
    public Guid GetGuid(int i) => this.reader.GetGuid(i);
    public short GetInt16(int i) => this.reader.GetInt16(i);
    public int GetInt32(int i) => this.reader.GetInt32(i);
    public long GetInt64(int i) => this.reader.GetInt64(i);
    public string GetName(int i) => this.reader.GetName(i);
    public int GetOrdinal(string name) => this.reader.GetOrdinal(name);
    public string GetString(int i) => this.reader.GetString(i);
    public object GetValue(int i) => this.reader.GetValue(i);
    public int GetValues(object[] values) => this.reader.GetValues(values);
}