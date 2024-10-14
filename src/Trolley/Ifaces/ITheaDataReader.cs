using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaDataReader : IDisposable, IAsyncDisposable
{
    IDataReader BaseDataReader { get; }

    Type GetFieldType(int ordinal);
    bool NextResult();
    Task<bool> NextResultAsync(CancellationToken cancellationToken = default);
    bool Read();
    Task<bool> ReadAsync(CancellationToken cancellationToken = default);
}
