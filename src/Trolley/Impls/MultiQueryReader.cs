using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly bool isNeedClose;
    private readonly DbContext dbContext;
    private readonly IOrmProvider ormProvider;
    private ITheaConnection connection;
    private ITheaCommand command;
    private ITheaDataReader reader;
    private List<ReaderAfter> readerAfters;

    private int readerIndex = 0;
    private List<NextReaderAfter> nextAfters;
    public MultiQueryReader(DbContext dbContext, ITheaConnection connection, ITheaCommand command, ITheaDataReader reader, List<ReaderAfter> readerAfters, bool isNeedClose)
    {
        this.dbContext = dbContext;
        this.connection = connection;
        this.ormProvider = dbContext.OrmProvider;
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
        else this.NextReader();
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
            totalCount = reader.ToValue<int>(this.dbContext);
        this.reader.NextResult();
        var dataList = new List<T>();
        (var pageNumber, var pageSize) = this.ReadList<T>(dataList, true);
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
        if (await this.reader.ReadAsync(cancellationToken))
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            result = (T)readerAfter.ReaderGetter.Invoke(this.reader);
            await this.NextReaderAsync(readerAfter, result, cancellationToken);
        }
        else await this.NextReaderAsync(cancellationToken);
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
        int totalCount = 0;
        if (await this.reader.ReadAsync(cancellationToken))
            totalCount = reader.ToValue<int>(this.dbContext);
        await this.reader.NextResultAsync(cancellationToken);

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
        var readerAfter = this.readerAfters[this.readerIndex];
        while (await this.reader.ReadAsync(cancellationToken))
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
        this.reader?.Dispose();
        this.reader = null;
        this.command?.Dispose();
        this.command = null;
        if (this.isNeedClose && this.connection != null)
            this.connection.Close();
    }
    public async ValueTask DisposeAsync()
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
        await this.reader.DisposeAsync();
        this.reader = null;
        this.command.Parameters.Clear();
        await this.command.DisposeAsync();
        this.command = null;
        if (this.isNeedClose && this.connection != null)
            await this.connection.CloseAsync();
    }
    private void NextReader()
    {
        this.reader.NextResult();
        this.readerIndex++;
        if (this.readerIndex == this.readerAfters.Count)
            this.Dispose();
    }
    private void NextReader(ReaderAfter readerAfter, object target)
    {
        var visitor = readerAfter.QueryVisitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, readerAfter.IsSingle, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
                IsSingle = readerAfter.IsSingle,
                Target = target
            });
        }
        this.reader.NextResult();
        this.readerIndex++;

        if (this.readerIndex >= this.readerAfters.Count)
        {
            if (this.nextAfters != null && this.nextAfters.Count > 0)
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
                using var includeReader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
                foreach (var nextAfter in this.nextAfters)
                {
                    nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader, nextAfter.IsSingle);
                }
            }
            this.Dispose();
        }
    }
    private async Task NextReaderAsync(CancellationToken cancellationToken = default)
    {
        await this.reader.NextResultAsync(cancellationToken);
        this.readerIndex++;
        if (this.readerIndex >= this.readerAfters.Count)
            await this.DisposeAsync();
    }
    private async Task NextReaderAsync(ReaderAfter readerAfter, object target, CancellationToken cancellationToken = default)
    {
        var visitor = readerAfter.QueryVisitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, readerAfter.IsSingle, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
                IsSingle = readerAfter.IsSingle,
                Target = target
            });
        }

        await this.reader.NextResultAsync(cancellationToken);
        this.readerIndex++;

        if (this.readerIndex >= this.readerAfters.Count)
        {
            if (this.nextAfters != null && this.nextAfters.Count > 0)
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

                using var includeReader = await this.command.ExecuteReaderAsync(CommandSqlType.Select, CommandBehavior.SequentialAccess, cancellationToken);
                foreach (var nextAfter in this.nextAfters)
                {
                    nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader, nextAfter.IsSingle);
                }
                await includeReader.NextResultAsync(cancellationToken);
            }
            await this.DisposeAsync();
        }
    }
    struct NextReaderAfter
    {
        public Type TargetType { get; set; }
        public string Sql { get; set; }
        public IQueryVisitor Visitor { get; set; }
        public bool IsSingle { get; set; }
        public object Target { get; set; }
    }
}
