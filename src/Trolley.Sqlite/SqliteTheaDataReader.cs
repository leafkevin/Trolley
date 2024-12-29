using System;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.Sqlite;

class SqliteTheaDataReader : ITheaDataReader
{
    private readonly SQLiteDataReader reader;
    public IDataReader BaseDataReader => this.reader;
    public int FieldCount => this.reader.FieldCount;

    public SqliteTheaDataReader(SQLiteDataReader reader) => this.reader = reader;

    public string GetName(int index) => this.reader.GetName(index);
    public object GetValue(int index) => this.reader.GetValue(index);
    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);
    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken)
        => this.reader.ReadAsync(cancellationToken);
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
}