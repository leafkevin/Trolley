using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryReader : IQueryReader
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbCommand command;
    private readonly IDataReader reader;

    public QueryReader(IOrmDbFactory dbFactory, TheaConnection connection, IDbCommand command, IDataReader reader)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.command = command;
        this.reader = reader;
    }

    #region 同步方法
    public TEntity Read<TEntity>()
    {
        var entityType = typeof(TEntity);
        var result = default(TEntity);
        if (reader.Read())
        {
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(dbFactory, connection);
            else result = reader.To<TEntity>();
        }
        this.ReadNextResult();
        return result;
    }
    public List<TEntity> ReadList<TEntity>()
    {
        var entityType = typeof(TEntity);
        var result = new List<TEntity>();
        if (entityType.IsEntityType())
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>(dbFactory, connection));
            }
        }
        else
        {
            while (reader.Read())
            {
                result.Add(reader.To<TEntity>());
            }
        }
        this.ReadNextResult();
        return result;
    }
    public IPagedList<TEntity> ReadPageList<TEntity>()
    {
        var recordsTotal = this.Read<int>();
        return new PagedList<TEntity>(recordsTotal, this.ReadList<TEntity>());
    }
    private void ReadNextResult()
    {
        if (!this.reader.NextResult())
        {
            this.reader.Dispose();
            this.Dispose();
        }
    }
    public void Dispose()
    {
        if (this.reader != null)
        {
            if (!this.reader.IsClosed) this.command?.Cancel();
            this.reader.Dispose();
        }
        if (this.command != null) this.command.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region 异步方法
    public async Task<TEntity> ReadAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var result = default(TEntity);
        if (this.reader is not DbDataReader dbReader)
            throw new Exception("当前数据库驱动不支持异步Reader操作");

        if (await dbReader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType())
                result = reader.To<TEntity>(dbFactory, connection);
            else result = reader.To<TEntity>();
        }
        await this.ReadNextResultAsync(dbReader, cancellationToken);
        return result;
    }
    public async Task<List<TEntity>> ReadListAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var result = new List<TEntity>();
        if (this.reader is not DbDataReader dbReader)
            throw new Exception("当前数据库驱动不支持异步Reader操作");

        while (await dbReader.ReadAsync(cancellationToken))
        {
            if (entityType.IsEntityType())
                result.Add(dbReader.To<TEntity>(dbFactory, connection));
            else result.Add(dbReader.To<TEntity>());
        }
        await this.ReadNextResultAsync(dbReader, cancellationToken);
        return result;
    }
    public async Task<IPagedList<TEntity>> ReadPageListAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var recordsTotal = await this.ReadAsync<int>(cancellationToken);
        return new PagedList<TEntity>(recordsTotal, await this.ReadListAsync<TEntity>(cancellationToken));
    }
    public async Task ReadNextResultAsync(DbDataReader dbReader, CancellationToken cancellationToken = default)
    {
        if (!await dbReader.NextResultAsync(cancellationToken))
        {
            await dbReader.DisposeAsync();
            await this.DisposeAsync();
        }
    }
    private async ValueTask DisposeAsync()
    {
        if (this.reader is not DbDataReader dbReader)
            throw new Exception("当前数据库驱动不支持异步Reader操作");
        if (dbReader != null)
        {
            if (!dbReader.IsClosed) this.command?.Cancel();
            await dbReader.DisposeAsync();
        }
        if (this.command != null && this.command is DbCommand dbCommand)
            await dbCommand.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    #endregion
}
