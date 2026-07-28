using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaDataReader : IDisposable, IAsyncDisposable
{
    IDataReader BaseDataReader { get; }
    int FieldCount { get; }

    string GetName(int index);
    Type GetFieldType(int ordinal);

    object GetValue(int index);
    T GetFieldValue<T>(int ordinal);
    Task<T> GetFieldValueAsync<T>(int ordinal);
    Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken);
  
    bool IsDBNull(int ordinal);
    Task<bool> IsDBNullAsync(int ordinal);
    Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken);

    bool NextResult();
    Task<bool> NextResultAsync(CancellationToken cancellationToken = default);
    bool Read();
    Task<bool> ReadAsync(CancellationToken cancellationToken = default);
}