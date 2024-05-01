using System;
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
    private readonly bool isNeedClose;
    private readonly IOrmProvider ormProvider;
    private IDbCommand command;
    private IDataReader reader;
    private List<ReaderAfter> readerAfters;

    private int readerIndex = 0;
    private List<NextReaderAfter> nextAfters;
    public MultiQueryReader(IOrmProvider ormProvider, IDbCommand command, IDataReader reader, List<ReaderAfter> readerAfters, bool isNeedClose)
    {
        this.ormProvider = ormProvider;
        this.command = command;
        this.reader = reader;
        this.readerAfters = readerAfters;
        this.isNeedClose = isNeedClose;
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
        var readerAfter = this.readerAfters[this.readerIndex];
        while (this.reader.Read())
        {
            result.Add((T)readerAfter.ReaderGetter.Invoke(this.reader));
        }
        this.NextReader(readerAfter, result);
        return result;
    }
    public IPagedList<T> ReadPageList<T>()
    {
        int totalCount = 0;
        if (this.reader.Read())
            totalCount = reader.To<int>(this.ormProvider);
        this.reader.NextResult();
        var dataList = new List<T>();

        var readerAfter = this.readerAfters[this.readerIndex];
        while (this.reader.Read())
        {
            dataList.Add((T)readerAfter.ReaderGetter.Invoke(this.reader));
        }
        var pageNumber = readerAfter.PageNumber;
        var pageSize = readerAfter.PageSize;
        this.NextReader(readerAfter, dataList);

        return new PagedList<T>
        {
            Data = dataList,
            Count = dataList.Count,
            TotalCount = totalCount,
            PageNumber = pageNumber,
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
            totalCount = reader.To<int>(this.ormProvider);
        await dbReader.NextResultAsync(cancellationToken);

        var dataList = new List<T>();
        (var pageIndex, var pageSize) = await this.ReadListAsync(dataList, true, cancellationToken);
        return new PagedList<T>
        {
            Data = dataList,
            TotalCount = totalCount,
            PageNumber = pageIndex,
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
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        if (this.command.Parameters != null && this.command.Parameters.Count > 0)
            dbParameters = this.command.Parameters.Cast<IDbDataParameter>().ToList();
        return this.command.CommandText;
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
            pageIndex = readerAfter.PageNumber;
            pageSize = readerAfter.PageSize;
        }
        this.NextReader(readerAfter, dataList);
        return (pageIndex, pageSize);
    }
    private async Task<(int, int)> ReadListAsync<T>(List<T> dataList, bool isPaged, CancellationToken cancellationToken = default)
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
            pageIndex = readerAfter.PageNumber;
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
        this.reader = null;
        this.command?.Dispose();
        this.command = null;
        if (this.isNeedClose && connection != null)
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
        {
            await dbReader.DisposeAsync();
            this.reader = null;
        }
        if (dbCommand != null)
        {
            await dbCommand.DisposeAsync();
            this.command = null;
        }
        if (this.isNeedClose && connection != null)
            await connection.DisposeAsync();
    }
    private void NextReader(ReaderAfter readerAfter, object target)
    {
        var visitor = readerAfter.QueryVisitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
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
            visitor.NextDbParameters.CopyTo(this.command.Parameters);
            using var includeReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            foreach (var nextAfter in this.nextAfters)
            {
                nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader);
            }
            if (!includeReader.NextResult())
                this.Dispose();
        }
        else this.Dispose();
    }
    private async Task NextReaderAsync(ReaderAfter readerAfter, object target, CancellationToken cancellationToken = default)
    {
        var visitor = readerAfter.QueryVisitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
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
            visitor.NextDbParameters.CopyTo(this.command.Parameters);
            if (this.command is not DbCommand dbCommand)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            using var includeReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            foreach (var nextAfter in this.nextAfters)
            {
                nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader);
            }
            if (!await includeReader.NextResultAsync(cancellationToken))
                await this.DisposeAsync();
        }
        else await this.DisposeAsync();
    }
    struct NextReaderAfter
    {
        public Type TargetType { get; set; }
        public string Sql { get; set; }
        public IQueryVisitor Visitor { get; set; }
        public object Target { get; set; }
    }
}
