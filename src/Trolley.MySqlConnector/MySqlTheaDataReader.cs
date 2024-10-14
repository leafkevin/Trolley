using MySqlConnector;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

class MySqlTheaDataReader : ITheaDataReader
{
    private readonly MySqlDataReader reader;
    public IDataReader BaseDataReader => this.reader;

    public MySqlTheaDataReader(MySqlDataReader reader) => this.reader = reader;

    public void Close() => this.reader.Close();
    public Task CloseAsync()
    {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return this.reader.CloseAsync();
#else
        return this.reader.DisposeAsync();
#endif
    }
    public void Dispose() => this.reader.Dispose();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
	public ValueTask DisposeAsync() => this.reader.DisposeAsync();
#else
    public ValueTask DisposeAsync() => new ValueTask(this.reader.DisposeAsync());
#endif

    public Type GetFieldType(int ordinal) => this.reader.GetFieldType(ordinal);
    public bool NextResult() => this.reader.NextResult();
    public Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => this.reader.NextResultAsync(cancellationToken);
    public bool Read() => this.reader.Read();
    public Task<bool> ReadAsync(CancellationToken cancellationToken)
        => this.reader.ReadAsync(cancellationToken);
}
