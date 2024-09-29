using System;
using System.Data;
//#if NET6_0_OR_GREATER
//using System.Data.Common;
//#endif
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaConnection : IDisposable, IAsyncDisposable
{
    string ConnectionId { get; }
    string DbKey { get; }
    IDbConnection BaseConnection { get; }

    string ConnectionString { get; set; }
    int ConnectionTimeout { get; }
    string Database { get; }
    ConnectionState State { get; }

    string ServerVersion { get; }
    bool CanCreateBatch { get; }

    Action<ConectionEventArgs> OnOpening { get; set; }
    Action<ConectionEventArgs> OnOpened { get; set; }
    Action<ConectionEventArgs> OnClosing { get; set; }
    Action<ConectionEventArgs> OnClosed { get; set; }

    void ChangeDatabase(string databaseName);
    Task ChangeDatabaseAsync(string databaseName, CancellationToken cancellationToken = default);
    ITheaCommand CreateCommand(IDbCommand command);
    void Open();
    Task OpenAsync(CancellationToken cancellationToken = default);
    void Close();
    Task CloseAsync();

//#if NET6_0_OR_GREATER
//    DbBatch CreateBatch();
//#endif
    ITheaTransaction BeginTransaction();
    ITheaTransaction BeginTransaction(IsolationLevel il);
    ValueTask<ITheaTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    ValueTask<ITheaTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default);
    DataTable GetSchema(string collectionName = default, string[] restrictionValues = default);
    Task<DataTable> GetSchemaAsync(string collectionName = default, string[] restrictionValues = default, CancellationToken cancellationToken = default);
}
