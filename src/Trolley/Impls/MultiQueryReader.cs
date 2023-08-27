﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly IDbCommand command;
    private readonly IDataReader reader;
    private int readerIndex = 0;
    private List<ReaderAfter> readerAfters;
    private List<NextReaderAfter> nextAfters;
    public MultiQueryReader(IDbCommand command, IDataReader reader, List<ReaderAfter> readerAfters = null)
    {
        this.command = command;
        this.reader = reader;
        this.readerAfters = readerAfters;
    }
    public T ReadFirst<T>()
    {
        T result = default;
        if (this.reader.Read())
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            result = (T)readerAfter.ReaderGetter.Invoke(this.reader);
            this.NextReader(readerAfter, result);
        }
        return result;
    }
    public List<T> Read<T>()
    {
        var result = new List<T>();
        this.ReadList(result, false);
        return result;
    }
    public IPagedList<T> ReadPageList<T>()
    {
        int totalCount = 0;
        if (this.reader.Read())
            totalCount = reader.To<int>();
        this.reader.NextResult();
        var dataList = new List<T>();
        (var pageIndex, var pageSize) = this.ReadList(dataList, true);
        return new PagedList<T>
        {
            Data = dataList,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
    public Dictionary<TKey, TValue> ReadDictionary<TEntity, TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        return this.Read<TEntity>().ToDictionary(keySelector, valueSelector);
    }
    public async Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default)
    {
        T result = default;
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (await dbReader.ReadAsync(cancellationToken))
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            result = (T)readerAfter.ReaderGetter.Invoke(this.reader);
            await this.NextReaderAsync(readerAfter, result, cancellationToken);
        }
        return result;
    }
    public async Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        await this.ReadListAsync(result, false, cancellationToken);
        return result;
    }
    public async Task<IPagedList<T>> ReadPageListAsync<T>(CancellationToken cancellationToken = default)
    {
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        int totalCount = 0;
        if (await dbReader.ReadAsync(cancellationToken))
            totalCount = reader.To<int>();
        await dbReader.NextResultAsync(cancellationToken);

        var dataList = new List<T>();
        (var pageIndex, var pageSize) = await this.ReadListAsync(dataList, true, cancellationToken);
        return new PagedList<T>
        {
            Data = dataList,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }
    public async Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TEntity, TKey, TValue>(Func<TEntity, TKey> keySelector, Func<TEntity, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));

        var dataList = await this.ReadAsync<TEntity>(cancellationToken);
        return dataList.ToDictionary(keySelector, valueSelector);
    }
    private (int, int) ReadList<T>(List<T> dataList, bool isPaged)
    {
        int pageIndex = 0;
        int pageSize = 0;
        var readerAfter = this.readerAfters[this.readerIndex];
        while (this.reader.Read())
        {
            dataList.Add((T)readerAfter.ReaderGetter.Invoke(this.reader));
        }
        if (isPaged)
        {
            pageIndex = readerAfter.PageIndex;
            pageSize = readerAfter.PageSize;
        }
        this.NextReader(readerAfter, dataList);
        return (pageIndex, pageSize);
    }
    private async Task<(int, int)> ReadListAsync<T>(List<T> dataList, bool isPaged, CancellationToken cancellationToken)
    {
        int pageIndex = 0;
        int pageSize = 0;
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        var readerAfter = this.readerAfters[this.readerIndex];
        while (await dbReader.ReadAsync(cancellationToken))
        {
            dataList.Add((T)readerAfter.ReaderGetter.Invoke(this.reader));
        }
        if (isPaged)
        {
            pageIndex = readerAfter.PageIndex;
            pageSize = readerAfter.PageSize;
        }
        await this.NextReaderAsync(readerAfter, dataList, cancellationToken);
        return (pageIndex, pageSize);
    }
    public void Dispose()
    {
        if (this.readerAfters != null)
        {
            this.readerAfters.Clear();
            this.readerAfters = null;
        }
        if (this.nextAfters != null)
        {
            this.nextAfters.Clear();
            this.nextAfters = null;
        }
        var connection = this.command?.Connection;
        this.reader?.Dispose();
        this.command?.Dispose();
        connection?.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        if (this.command is not DbCommand dbCommand)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (this.readerAfters != null)
        {
            this.readerAfters.Clear();
            this.readerAfters = null;
        }
        if (this.nextAfters != null)
        {
            this.nextAfters.Clear();
            this.nextAfters = null;
        }
        var connection = dbCommand?.Connection;
        if (dbReader != null)
            await dbReader.DisposeAsync();
        if (dbCommand != null)
            await dbCommand.DisposeAsync();
        if (connection != null)
            await connection.DisposeAsync();
    }
    private void NextReader(ReaderAfter readerAfter, object target)
    {
        if (readerAfter.QueryVisitor != null && readerAfter.QueryVisitor.BuildIncludeSql(target, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                Sql = sql,
                Visitor = readerAfter.QueryVisitor,
                Target = target
            });
        }
        if (this.reader.NextResult())
            this.readerIndex++;
        else if (this.readerIndex == this.readerAfters.Count - 1
            && this.nextAfters != null && this.nextAfters.Count > 0)
        {
            var builder = new StringBuilder();
            foreach (var nextAfter in this.nextAfters)
            {
                if (builder.Length > 0) builder.Append(';');
                builder.Append(nextAfter.Sql);
            }
            this.command.CommandText = builder.ToString();
            this.command.Parameters.Clear();
            using var includeReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            foreach (var nextAfter in this.nextAfters)
            {
                nextAfter.Visitor.SetIncludeValues(nextAfter.Target, includeReader);
            }
            if (!includeReader.NextResult())
                this.Dispose();
        }
        else this.Dispose();
    }
    private async Task NextReaderAsync(ReaderAfter readerAfter, object target, CancellationToken cancellationToken)
    {
        if (readerAfter.QueryVisitor != null && readerAfter.QueryVisitor.BuildIncludeSql(target, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                Sql = sql,
                Visitor = readerAfter.QueryVisitor,
                Target = target
            });
        }
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (await dbReader.NextResultAsync(cancellationToken))
            this.readerIndex++;
        else if (this.readerIndex == this.readerAfters.Count - 1
            && this.nextAfters != null && this.nextAfters.Count > 0)
        {
            var builder = new StringBuilder();
            foreach (var nextAfter in this.nextAfters)
            {
                if (builder.Length > 0) builder.Append(';');
                builder.Append(nextAfter.Sql);
            }
            this.command.CommandText = builder.ToString();
            this.command.Parameters.Clear();
            if (this.command is not DbCommand dbCommand)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            using var includeReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            foreach (var nextAfter in this.nextAfters)
            {
                nextAfter.Visitor.SetIncludeValues(nextAfter.Target, includeReader);
            }
            if (!await includeReader.NextResultAsync(cancellationToken))
                await this.DisposeAsync();
        }
        else await this.DisposeAsync();
    }
    struct NextReaderAfter
    {
        public string Sql { get; set; }
        public IQueryVisitor Visitor { get; set; }
        public object Target { get; set; }
    }
}
