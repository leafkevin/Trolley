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

    public SqlServerTheaDataReader(SqlDataReader reader) => this.reader = reader;

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
