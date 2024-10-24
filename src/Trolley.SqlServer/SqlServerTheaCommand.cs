﻿using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

class SqlServerTheaCommand : ITheaCommand
{
    private readonly SqlCommand command;
    private ITheaConnection connection;
    private ITheaTransaction transaction;
    private int index = -1;

    public string DbKey => this.connection?.DbKey;
    public string CommandId { get; private set; }
    public IDbCommand BaseCommand => this.command;
    public bool IsNeedClose => this.transaction == null;

    public string CommandText { get => this.command.CommandText; set => this.command.CommandText = value; }
    public int CommandTimeout { get => this.command.CommandTimeout; set => this.command.CommandTimeout = value; }
    public CommandType CommandType { get => this.command.CommandType; set => this.command.CommandType = value; }
    public IDataParameterCollection Parameters => this.command.Parameters;
    public UpdateRowSource UpdatedRowSource { get => this.command.UpdatedRowSource; set => this.command.UpdatedRowSource = value; }
    public bool DesignTimeVisible { get => this.command.DesignTimeVisible; set => this.command.DesignTimeVisible = value; }
    public ITheaConnection Connection
    {
        get => this.connection;
        set
        {
            this.connection = value;
            this.BaseCommand.Connection = value.BaseConnection;
        }
    }
    public ITheaTransaction Transaction
    {
        get => this.transaction;
        set
        {
            this.transaction = value;
            this.BaseCommand.Transaction = value?.BaseTransaction ?? null;
        }
    }
    public Action<CommandEventArgs> OnExecuting { get; set; }
    public Action<CommandCompletedEventArgs> OnExecuted { get; set; }

    public SqlServerTheaCommand(SqlCommand command) : this(command, null, null) { }
    public SqlServerTheaCommand(SqlCommand command, ITheaConnection connection, ITheaTransaction transaction)
    {
        this.CommandId = Guid.NewGuid().ToString("N");
        this.command = command;
        this.Connection = connection;
        this.transaction = transaction;
    }

    public void Cancel() => this.command.Cancel();
    public IDbDataParameter CreateParameter() => this.command.CreateParameter();

    public int ExecuteNonQuery(CommandSqlType sqlType)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            recordsAffected = this.command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteNonQueryAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            recordsAffected = await this.command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return recordsAffected;
    }

    public ITheaDataReader ExecuteReader(CommandSqlType sqlType, CommandBehavior behavior = default)
    {
        this.index++;
        bool isNeedClose = this.IsNeedClose;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        SqlDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            reader = this.command.ExecuteReader(behavior);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return new SqlServerTheaDataReader(reader);
    }
    public async Task<ITheaDataReader> ExecuteReaderAsync(CommandSqlType sqlType, CommandBehavior behavior = default, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        SqlDataReader reader = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            reader = await this.command.ExecuteReaderAsync(behavior, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return new SqlServerTheaDataReader(reader);
    }
    public object ExecuteScalar(CommandSqlType sqlType)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            result = this.command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) this.connection.Close();
            throw exception;
        }
        return result;
    }
    public async Task<object> ExecuteScalarAsync(CommandSqlType sqlType, CancellationToken cancellationToken = default)
    {
        this.index++;
        var createdAt = DateTime.Now;
        this.OnExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = this.DbKey,
            CommandId = this.CommandId,
            ConnectionString = this.Connection.ConnectionString,
            Sql = this.CommandText,
            DbParameters = this.Parameters,
            Index = this.index,
            SqlType = sqlType
        });
        object result = null;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            result = await this.command.ExecuteScalarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            this.OnExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = this.DbKey,
                CommandId = this.CommandId,
                ConnectionString = this.Connection.ConnectionString,
                Sql = this.CommandText,
                DbParameters = this.Parameters,
                Index = this.index,
                SqlType = sqlType,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (this.IsNeedClose) await this.connection.CloseAsync();
            throw exception;
        }
        return result;
    }
    public void Dispose()
    {
        this.command.CommandText = null;
        this.command.Parameters.Clear();
        this.command.Dispose();
    }
    public ValueTask DisposeAsync()
    {
        this.command.CommandText = null;
        this.command.Parameters.Clear();
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return this.command.DisposeAsync();
#else
        this.command.Dispose();
        return default;
#endif
    }
}