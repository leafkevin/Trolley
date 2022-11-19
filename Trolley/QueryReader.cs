using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
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

        //while (reader.Read())
        //{
        //    reader.To<TEntity>();
        //    var objResult = func?.Invoke(reader);
        //    if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
        //    else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
        //}
        //this.ReadNextResult();
        return result;
    }
    public List<TEntity> ReadList<TEntity>()
    {
        var entityType = typeof(TEntity);
        var result = new List<TEntity>();
        var func = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, this.reader, entityType, entityType);
        while (reader.Read())
        {
            var objResult = func?.Invoke(reader);
            if (objResult == null) continue;
            if (objResult is TEntity) result.Add((TEntity)objResult);
            else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
        }
        this.ReadNextResult();
        return result;
    }
    public IPagedList<TEntity> ReadPageList<TEntity>()
    {
        var recordsTotal = this.Read<int>();
        return new PagedList<TEntity>(recordsTotal, this.ReadList<TEntity>());
    }
    public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
    {
        throw new NotImplementedException();
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
        var func = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, this.reader, entityType, entityType);
        var readerAsync = this.reader as DbDataReader;
        while (await readerAsync.ReadAsync(cancellationToken))
        {
            var objResult = func?.Invoke(reader);
            if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
            else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
        }
        await this.ReadNextResultAsync(cancellationToken);
        return result;
    }
    public async Task<List<TEntity>> ReadListAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var result = new List<TEntity>();
        var func = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, this.reader, entityType, entityType);
        var readerAsync = this.reader as DbDataReader;
        while (await readerAsync.ReadAsync(cancellationToken))
        {
            var objResult = func?.Invoke(reader);
            if (objResult == null) continue;
            if (objResult is TEntity) result.Add((TEntity)objResult);
            else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
        }
        await this.ReadNextResultAsync(cancellationToken);
        return result;
    }
    public async Task<IPagedList<TEntity>> ReadPageListAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var recordsTotal = await this.ReadAsync<int>();
        return new PagedList<TEntity>(recordsTotal, await this.ReadListAsync<TEntity>(cancellationToken));
    }
    public async Task ReadNextResultAsync(CancellationToken cancellationToken = default)
    {
        var readerAsync = this.reader as DbDataReader;
        if (!(await readerAsync.NextResultAsync(cancellationToken)))
        {
            await readerAsync.DisposeAsync();
            await this.DisposeAsync();
        }
    }
    public Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TKey, TValue>(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task DisposeAsync()
    {
        if (this.reader != null)
        {
            var readerAsync = this.reader as DbDataReader;
            if (!this.reader.IsClosed) this.command?.Cancel();
            await readerAsync.DisposeAsync();
        }
        if (this.command != null) this.command.Dispose();
        GC.SuppressFinalize(this);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        throw new NotImplementedException();
    }
    #endregion
}
