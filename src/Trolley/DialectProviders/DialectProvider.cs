using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class DialectProvider
{
    protected internal string dbKey => this.DbContext.DbKey;
    protected internal string connectionString => this.DbContext.ConnectionString;
    protected internal TheaDatabase database => this.DbContext.Database;
    protected internal ITheaConnection connection => this.DbContext.Connection;
    protected internal ITheaTransaction transaction => this.DbContext.Transaction;
    protected internal string defaultTableSchema => this.DbContext.DbKey;
    protected internal IOrmProvider ormProvider => this.database.OrmProvider;
    protected internal IEntityMapProvider entityMapProvider => this.database.EntityMapProvider;
    protected internal ITableShardingProvider tableShardingProvider => this.database.TableShardingProvider;
    protected internal IDbInterceptor interceptor => this.DbContext.Interceptor;
    protected internal OrmDbFactoryOptions options => this.DbContext.Options;

    #region Properties
    public DbContext DbContext { get; set; }
    #endregion

    #region UseMasterCommand/UseSlaveCommand
    public (bool, ITheaConnection, ITheaCommand) UseMasterCommand(ICommandContext commandContext = null)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.transaction != null)
            connection = this.connection;
        else
        {
            isNeedClose = true;
            var connString = this.connectionString ?? this.database.Select();
            connection = this.CreateConnection(connString);
        }
        command = commandContext?.Command ?? this.ormProvider.CreateCommand();
        command.Connection = connection;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.options.CommandTimeout;
        command.Transaction = this.transaction;
        command.DbInterceptor = this.interceptor;
        return (isNeedClose, connection, command);
    }
    public (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(ICommandContext commandContext = null)
    {
        bool isNeedClose = false;
        ITheaConnection connection;
        ITheaCommand command;
        if (this.transaction != null)
            connection = this.connection;
        else
        {
            isNeedClose = true;
            var connString = this.connectionString ?? this.database.SelectSlave();
            connection = this.CreateConnection(connString);
        }
        command = commandContext?.Command ?? this.ormProvider.CreateCommand();
        command.Connection = connection;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = this.options.CommandTimeout;
        command.Transaction = this.transaction;
        command.DbInterceptor = this.interceptor;
        return (isNeedClose, connection, command);
    }
    private ITheaConnection CreateConnection(string connectionString)
    {
        var isNext = this.interceptor?.ConnectionCreating() ?? true;
        if (isNext)
        {
            var connection = this.ormProvider.CreateConnection(this.dbKey, connectionString);
            connection.DbInterceptor = this.interceptor;
            return connection;
        }
        return null;
    }
    #endregion

    #region Execute
    public int Execute(bool isNeedClose, ITheaConnection connection, ITheaCommand command)
    {
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.RawExecute);
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> ExecuteAsync(bool isNeedClose, ITheaConnection connection, ITheaCommand command, CancellationToken cancellationToken = default)
    {
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.RawExecute, cancellationToken);
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Others   
    public void BeginTransaction()
    {
        if (this.transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.DbContext.Connection ??= this.CreateConnection(this.database.Select());
        this.connection.Open();
        this.DbContext.Transaction = this.connection.BeginTransaction();
    }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction != null)
            throw new Exception("上一个事务还没有完成，无法开启新事务");
        this.DbContext.Connection ??= this.CreateConnection(this.database.Select());
        await this.connection.OpenAsync(cancellationToken);
        this.DbContext.Transaction = await this.connection.BeginTransactionAsync(cancellationToken);
    }
    public void Commit()
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        this.transaction.Commit();
        this.connection.Close();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成提交");
        await this.transaction.CommitAsync(cancellationToken);
        await this.connection.CloseAsync();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public void Rollback()
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        this.transaction.Rollback();
        this.connection.Close();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction == null)
            throw new Exception("还没有开启事务，无法完成回滚");
        await this.transaction.RollbackAsync(cancellationToken);
        await this.connection.CloseAsync();
        this.DbContext.Transaction = null;
        this.DbContext.Connection = null;
    }
    #endregion
}