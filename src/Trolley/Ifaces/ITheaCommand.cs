using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaCommand : IDbCommand, ICloneable, IAsyncDisposable
{
    string DbKey { get; }
    string CommandId { get; }
    new ITheaConnection Connection { get; set; }
    IDbCommand DbCommand { get; }
    IDbInterceptor Interceptor { get; set; }

    new ITheaDataReader ExecuteReader();
    new ITheaDataReader ExecuteReader(CommandBehavior behavior);

    Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);
    Task<ITheaDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default);
    Task<ITheaDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken = default);
    Task<object> ExecuteScalarAsync(CancellationToken cancellationToken = default);
}