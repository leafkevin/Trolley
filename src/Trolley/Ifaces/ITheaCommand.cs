using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ITheaCommand : IDisposable, IAsyncDisposable
{
    string DbKey { get; }
    string CommandId { get; }
    IDbCommand BaseCommand { get; }

    string CommandText { get; set; }
    int CommandTimeout { get; set; }
    CommandType CommandType { get; set; }
    ITheaConnection Connection { get; set; }
    IDataParameterCollection Parameters { get; }
    ITheaTransaction Transaction { get; set; }

    Action<CommandEventArgs> OnExecuting { get; set; }
    Action<CommandCompletedEventArgs> OnExecuted { get; set; }

    int ExecuteNonQuery(CommandSqlType sqlType);
    Task<int> ExecuteNonQueryAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default);
    ITheaDataReader ExecuteReader(CommandSqlType sqlType, CommandBehavior behavior = default);
    Task<ITheaDataReader> ExecuteReaderAsync(CommandSqlType sqlType, CommandBehavior behavior = default, CancellationToken cancellationToken = default);
    object ExecuteScalar(CommandSqlType sqlType);
    Task<object> ExecuteScalarAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default);
}