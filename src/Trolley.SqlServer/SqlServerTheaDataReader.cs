using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaDataReader : ITheaDataReader
{
    private readonly SqlDataReader reader;

    public string ReaderId { get; private set; }
    public int Depth => this.reader.Depth;
    public bool IsClosed => this.reader.IsClosed;
    public int RecordsAffected => this.reader.RecordsAffected;
    public int FieldCount => this.reader.FieldCount;
    public object this[string name] => this.reader[name];
    public object this[int ordinal] => this.reader[ordinal];
    public bool HasRows => this.reader.HasRows;

    public IDataReader DbDataReader => this.reader;
    public IDbInterceptor Interceptor { get; set; }

    public SqlServerTheaDataReader(SqlDataReader reader)
    {
        this.ReaderId = Guid.NewGuid().ToString("N");
        this.reader = reader;
    }
    public void Close()
    {
        this.Interceptor?.DataReaderClosing(this);
        this.reader.Close();
        this.Interceptor?.DataReaderClosed(this);
    }
    public async Task CloseAsync()
    {
        this.Interceptor?.DataReaderClosing(this);
        await this.reader.CloseAsync();
        this.Interceptor?.DataReaderClosed(this);
    }
    public void Dispose()
    {
        this.Interceptor?.DataReaderDisposing(this);
        this.reader.Dispose();
        this.Interceptor?.DataReaderDisposed(this);
    }
    public async ValueTask DisposeAsync()
    {
        this.Interceptor?.DataReaderDisposing(this);
        await this.reader.DisposeAsync();
        this.Interceptor?.DataReaderDisposed(this);
    }
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        => this.reader.ReadAsync(cancellationToken);
    public bool IsDBNull(int ordinal) => this.reader.IsDBNull(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal) => this.reader.IsDBNullAsync(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        => this.reader.IsDBNullAsync(ordinal, cancellationToken);
    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);
    public string GetDataTypeName(int ordinal) => this.reader.GetDataTypeName(ordinal);
    public IDataReader GetData(int ordinal) => this.reader.GetData(ordinal);

    public string GetName(int ordinal) => this.reader.GetName(ordinal);
    public int GetOrdinal(string name) => this.reader.GetOrdinal(name);
    public int GetValues(object[] values) => this.reader.GetValues(values);

    public object GetValue(int ordinal) => this.reader.GetValue(ordinal);
    public T GetFieldValue<T>(int ordinal) => this.reader.GetFieldValue<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal)
        => this.reader.GetFieldValueAsync<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        => this.reader.GetFieldValueAsync<T>(ordinal, cancellationToken);

    public bool GetBoolean(int ordinal) => this.reader.GetBoolean(ordinal);
    public byte GetByte(int ordinal) => this.reader.GetByte(ordinal);
    public long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        => this.reader.GetBytes(ordinal, fieldOffset, buffer, bufferoffset, length);
    public Stream GetStream(int ordinal) => this.reader.GetStream(ordinal);
    public TextReader GetTextReader(int ordinal) => this.reader.GetTextReader(ordinal);

    public char GetChar(int ordinal) => this.reader.GetChar(ordinal);
    public long GetChars(int ordinal, long fieldoffset, char[] buffer, int bufferoffset, int length)
        => this.reader.GetChars(ordinal, fieldoffset, buffer, bufferoffset, length);

    public DateTime GetDateTime(int ordinal) => this.reader.GetDateTime(ordinal);
    public TimeSpan GetTimeSpan(int ordinal) => this.reader.GetTimeSpan(ordinal);
    public decimal GetDecimal(int ordinal) => this.reader.GetDecimal(ordinal);
    public double GetDouble(int ordinal) => this.reader.GetFloat(ordinal);
    public float GetFloat(int ordinal) => this.reader.GetFloat(ordinal);
    public Guid GetGuid(int ordinal) => this.reader.GetGuid(ordinal);
    public short GetInt16(int ordinal) => this.reader.GetInt16(ordinal);
    public int GetInt32(int ordinal) => this.reader.GetInt32(ordinal);
    public long GetInt64(int ordinal) => this.reader.GetInt64(ordinal);
    public string GetString(int ordinal) => this.reader.GetString(ordinal);

    public DataTable GetSchemaTable() => this.reader.GetSchemaTable();

    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken = default)
        => this.reader.NextResultAsync(cancellationToken);
}