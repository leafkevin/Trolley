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

namespace Trolley;

public interface ITheaDataReader : IDisposable
{
    IDataReader BaseDataReader { get; }

    bool HasRows { get; }
    object this[string name] { get; }
    object this[int ordinal] { get; }
    bool IsClosed { get; }
    int FieldCount { get; }
    int Depth { get; }
    int RecordsAffected { get; }
    int VisibleFieldCount { get; }

    void Close();
    Task CloseAsync();
	public ValueTask DisposeAsync();
    bool GetBoolean(int ordinal);
    byte GetByte(int ordinal);
    long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length);
    char GetChar(int ordinal);
    long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length);
#if NET5_0_OR_GREATER
    Task<ReadOnlyCollection<DbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = default);
#endif
    DbDataReader GetData(int ordinal);
    string GetDataTypeName(int ordinal);
    DateTime GetDateTime(int ordinal);
    decimal GetDecimal(int ordinal);
    double GetDouble(int ordinal);
    IEnumerator GetEnumerator();
    Type GetFieldType(int ordinal);
    T GetFieldValue<T>(int ordinal);
    Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken = default);
    float GetFloat(int ordinal);
    Guid GetGuid(int ordinal);
    short GetInt16(int ordinal);
    int GetInt32(int ordinal);
    long GetInt64(int ordinal);
    string GetName(int ordinal);
    int GetOrdinal(string name);
    Type GetProviderSpecificFieldType(int ordinal);
    object GetProviderSpecificValue(int ordinal);
    int GetProviderSpecificValues(object[] values);

    DataTable GetSchemaTable();
    Task<DataTable> GetSchemaTableAsync(CancellationToken cancellationToken = default);
    Stream GetStream(int ordinal);
    string GetString(int ordinal);
    TextReader GetTextReader(int ordinal);
    object GetValue(int ordinal);
    int GetValues(object[] values);
    bool IsDBNull(int ordinal);
    Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken = default);
    bool NextResult();
    Task<bool> NextResultAsync(CancellationToken cancellationToken = default);
    bool Read();
    Task<bool> ReadAsync(CancellationToken cancellationToken = default);
}
