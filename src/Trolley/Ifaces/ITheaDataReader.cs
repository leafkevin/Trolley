using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaDataReader : IDataReader, IDisposable, IAsyncDisposable
{
    IDataReader DbDataReader { get; }

    bool HasRows { get; }
    T GetFieldValue<T>(int ordinal);
    Task<T> GetFieldValueAsync<T>(int ordinal);
    Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken);
    Stream GetStream(int ordinal);
    TextReader GetTextReader(int ordinal);
    TimeSpan GetTimeSpan(int ordinal);
    Task<bool> IsDBNullAsync(int ordinal);
    Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken);

    Task<bool> NextResultAsync(CancellationToken cancellationToken = default);
    Task<bool> ReadAsync(CancellationToken cancellationToken = default);
}