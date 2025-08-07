using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaDataReader : ITheaDataReader
{
    private readonly SqlDataReader reader;
    public IDataReader BaseDataReader => this.reader;
    public int FieldCount => this.reader.FieldCount;

    public SqlServerTheaDataReader(SqlDataReader reader) => this.reader = reader;

    public string GetName(int index) => this.reader.GetName(index);
    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);

    public object GetValue(int index) => this.reader.GetValue(index);
    public T GetFieldValue<T>(int ordinal) => this.reader.GetFieldValue<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal) => this.reader.GetFieldValueAsync<T>(ordinal);
    public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        => this.reader.GetFieldValueAsync<T>(ordinal, cancellationToken);

    public bool IsDBNull(int ordinal) => this.reader.IsDBNull(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal) => this.reader.IsDBNullAsync(ordinal);
    public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        => this.reader.IsDBNullAsync(ordinal, cancellationToken);

    public void Close() => this.reader.Close();
    public Task CloseAsync()
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        => this.reader.CloseAsync();
#else
    {
        this.reader.Close();
        return Task.CompletedTask;
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

    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken)
        => this.reader.ReadAsync(cancellationToken);
}