using Microsoft.Data.SqlClient;
using System;
using System.Collections;
#if NET5_0_OR_GREATER
using System.Collections.ObjectModel;
#endif
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaDataReader : ITheaDataReader
{
    private readonly SqlDataReader reader;
    public IDataReader BaseDataReader => this.reader;

    public object this[int ordinal] => this.reader[ordinal];
    public object this[string name] => this.reader[name];
    public bool HasRows => this.reader.HasRows;
    public int VisibleFieldCount => this.reader.VisibleFieldCount;
    public int Depth => this.reader.Depth;
    public bool IsClosed => this.reader.IsClosed;
    public int RecordsAffected => this.reader.RecordsAffected;
    public int FieldCount => this.reader.FieldCount;

    public SqlServerTheaDataReader(SqlDataReader reader) => this.reader = reader;

    public void Close() => this.reader.Close();
    public Task CloseAsync()
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.reader.CloseAsync();
#else
    {
        try
        {
            this.Close();
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            return Task.FromException(e);
        }
    }
#endif
    public void Dispose() => this.reader.Dispose();
    public ValueTask DisposeAsync()
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.reader.DisposeAsync();
#else
    {
        this.reader.Dispose();
        return default;
    }
#endif
    public bool GetBoolean(int ordinal) => this.reader.GetBoolean(ordinal);
    public byte GetByte(int ordinal) => this.reader.GetByte(ordinal);
    public long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        => this.reader.GetBytes(ordinal, fieldOffset, buffer, bufferoffset, length);
    public char GetChar(int ordinal) => this.reader.GetChar(ordinal);
    public long GetChars(int ordinal, long fieldoffset, char[] buffer, int bufferoffset, int length)
        => this.reader.GetChars(ordinal, fieldoffset, buffer, bufferoffset, length);
#if NET5_0_OR_GREATER
    public Task<ReadOnlyCollection<DbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = default)
        => this.reader.GetColumnSchemaAsync(cancellationToken);
#endif
    public DbDataReader GetData(int ordinal) => this.reader.GetData(ordinal);

    public string GetDataTypeName(int ordinal) => this.reader.GetDataTypeName(ordinal);
    public DateTime GetDateTime(int ordinal) => this.reader.GetDateTime(ordinal);
    public decimal GetDecimal(int ordinal) => this.reader.GetDecimal(ordinal);
    public double GetDouble(int ordinal) => this.reader.GetDouble(ordinal);
    public IEnumerator GetEnumerator() => this.reader.GetEnumerator();
    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);
    public T GetFieldValue<T>(int ordinal) => this.reader.GetFieldValue<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal) => this.reader.GetFieldValueAsync<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        => this.reader.GetFieldValueAsync<T>(ordinal, cancellationToken);
    public float GetFloat(int ordinal) => this.reader.GetFloat(ordinal);
    public Guid GetGuid(int ordinal) => this.reader.GetGuid(ordinal);
    public short GetInt16(int ordinal) => this.reader.GetInt16(ordinal);
    public int GetInt32(int ordinal) => this.reader.GetInt32(ordinal);
    public long GetInt64(int ordinal) => this.reader.GetInt64(ordinal);
    public string GetName(int ordinal) => this.reader.GetName(ordinal);
    public int GetOrdinal(string name) => this.reader.GetOrdinal(name);
    public Type GetProviderSpecificFieldType(int ordinal) => this.reader.GetProviderSpecificFieldType(ordinal);
    public object GetProviderSpecificValue(int ordinal) => this.reader.GetProviderSpecificValue(ordinal);
    public int GetProviderSpecificValues(object[] values) => this.reader.GetProviderSpecificValues(values);
    public DataTable GetSchemaTable() => this.reader.GetSchemaTable();
    public Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default)
#if NETCOREAPP5_0_OR_GREATER
        => this.reader.GetSchemaTableAsync(cancellationToken);
#else
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<DataTable>(cancellationToken);
        try
        {
            return Task.FromResult(GetSchemaTable());
        }
        catch (Exception e)
        {
            return Task.FromException<DataTable>(e);
        }
    }
#endif
    public Stream GetStream(int ordinal) => this.reader.GetStream(ordinal);
    public string GetString(int ordinal) => this.reader.GetString(ordinal);
    public TextReader GetTextReader(int ordinal) => this.reader.GetTextReader(ordinal);
    public object GetValue(int ordinal) => this.reader.GetValue(ordinal);
    public int GetValues(object[] values) => this.reader.GetValues(values);
    public bool IsDBNull(int ordinal) => this.reader.IsDBNull(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal) => this.reader.IsDBNullAsync(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        => this.reader.IsDBNullAsync(ordinal, cancellationToken);
    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync() => this.reader.NextResultAsync();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync() => this.reader.ReadAsync();
    public Task<bool> ReadAsync(CancellationToken cancellationToken)
        => this.reader.ReadAsync(cancellationToken);
}
