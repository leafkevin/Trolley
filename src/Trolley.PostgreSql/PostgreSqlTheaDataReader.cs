using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Trolley.PostgreSql;

class PostgreSqlTheaDataReader : ITheaDataReader
{
    private readonly NpgsqlDataReader reader;
    public IDataReader BaseDataReader => this.reader;

    public PostgreSqlTheaDataReader(NpgsqlDataReader reader) => this.reader = reader;

    public void Close() => this.reader.Close();
    public Task CloseAsync() => this.reader.CloseAsync();
    public void Dispose() => this.reader.Dispose();
    public ValueTask DisposeAsync() => this.reader.DisposeAsync();
    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);
    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken)
        => this.reader.ReadAsync(cancellationToken);
}