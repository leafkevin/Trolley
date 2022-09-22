using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public partial class Repository : IRepository
{
    #region 字段
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;

    private IDbTransaction transaction;
    #endregion

    #region 属性
    public IDbTransaction Transaction => this.transaction;
    public IOrmProvider OrmProvider => this.connection.OrmProvider;
    #endregion

    #region 构造方法
    internal Repository(IOrmDbFactory dbFactory, TheaConnection connection)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
    }
    #endregion

    #region 同步方法
    public TEntity QueryFirst<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        return this.QueryFirstImplWith<TEntity>(sql, entityType, entityType, whereObj, cmdType);
    }
    public TTarget QueryFirst<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryFirstImplWith<TTarget>(sql, entityType, targetType, whereObj, cmdType);
    }
    public List<TEntity> Query<TEntity>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        return this.QueryImpl<TEntity>(entityType, entityType, whereObj);
    }
    public List<TTarget> Query<TEntity, TTarget>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryImpl<TTarget>(entityType, targetType, whereObj);
    }
    public List<TEntity> Query<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        return this.QueryImplWith<TEntity>(sql, entityType, entityType, whereObj, cmdType);
    }
    public List<TTarget> Query<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryImplWith<TTarget>(sql, entityType, targetType, whereObj, cmdType);
    }
    public List<TEntity> QueryAll<TEntity>()
    {
        var entityType = typeof(TEntity);
        return this.QueryImpl<TEntity>(entityType, entityType, null);
    }
    public List<TTarget> QueryAll<TEntity, TTarget>()
    {
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryImpl<TTarget>(entityType, targetType, null);
    }
    public IPagedList<TEntity> QueryPage<TEntity>(int pageIndex, int pageSize, string orderBy = null)
    {
        var entityType = typeof(TEntity);
        return this.QueryPageImpl<TEntity>(entityType, entityType, null, pageIndex, pageSize, orderBy);
    }
    public IPagedList<TTarget> QueryPage<TEntity, TTarget>(int pageIndex, int pageSize, string orderBy = null)
    {
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryPageImpl<TTarget>(entityType, targetType, null, pageIndex, pageSize, orderBy);
    }
    public IPagedList<TEntity> QueryPage<TEntity>(object whereObj, int pageIndex, int pageSize, string orderBy = null)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        return this.QueryPageImpl<TEntity>(entityType, entityType, whereObj, pageIndex, pageSize, orderBy);
    }
    public IPagedList<TTarget> QueryPage<TEntity, TTarget>(object whereObj, int pageIndex, int pageSize, string orderBy = null)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryPageImpl<TTarget>(entityType, targetType, whereObj, pageIndex, pageSize, orderBy);
    }
    public IPagedList<TEntity> QueryPage<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
        => this.QueryPageImplWith<TEntity>(sql, whereObj, cmdType);
    public IPagedList<TEntity> QueryPage<TEntity>(string rawSql, int pageIndex, int pageSize, string orderBy = null, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        var sql = this.BuildPageSql(rawSql, pageIndex, pageSize, orderBy);
        return this.QueryPageImplWith<TEntity>(sql, whereObj, cmdType);
    }
    public IQueryReader QueryMultiple(string sql, object whereObj = null, CommandType cmdType = CommandType.Text)
    {
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }

        return this.QueryMultipleImpl(command);
    }
    //public IQueryReader QueryMultiple(Action<IQueryBuilder> queryInitializer)
    //{
    //    if (queryInitializer == null)
    //        throw new ArgumentNullException("queryInitializer");

    //    var command = this.connection.CreateCommand();
    //    command.Transaction = this.transaction;
    //    var builder = new QueryBuilder(this.dbFactory, this.connection, command);
    //    queryInitializer(builder);
    //    return this.QueryMultipleImpl(builder.command);
    //}
    #endregion

    #region 异步方法
    public async Task<TEntity> QueryFirstAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        return await this.QueryFirstImplWithAsync<TEntity>(sql, entityType, entityType, whereObj, cmdType, cancellationToken);
    }
    public async Task<TTarget> QueryFirstAsync<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryFirstImplWithAsync<TTarget>(sql, entityType, targetType, whereObj, cmdType, cancellationToken);
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        return await this.QueryImplAsync<TEntity>(entityType, entityType, whereObj, cancellationToken);
    }
    public async Task<List<TTarget>> QueryAsync<TEntity, TTarget>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryImplAsync<TTarget>(entityType, targetType, whereObj, cancellationToken);
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        return await this.QueryImplWithAsync<TEntity>(sql, entityType, entityType, whereObj, cmdType, cancellationToken);
    }
    public async Task<List<TTarget>> QueryAsync<TEntity, TTarget>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryImplWithAsync<TTarget>(sql, entityType, targetType, whereObj, cmdType, cancellationToken);
    }
    public async Task<List<TEntity>> QueryAllAsync<TEntity>(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        return await this.QueryImplAsync<TEntity>(entityType, entityType, null, cancellationToken);
    }
    public async Task<List<TTarget>> QueryAllAsync<TEntity, TTarget>(CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryImplAsync<TTarget>(entityType, targetType, null, cancellationToken);
    }
    public async Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        return await this.QueryPageImplAsync<TEntity>(entityType, entityType, null, pageIndex, pageSize, orderBy, cancellationToken);
    }
    public async Task<IPagedList<TTarget>> QueryPageAsync<TEntity, TTarget>(int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryPageImplAsync<TTarget>(entityType, targetType, null, pageIndex, pageSize, orderBy, cancellationToken);
    }
    public async Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(object whereObj, int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        return await this.QueryPageImplAsync<TEntity>(entityType, entityType, whereObj, pageIndex, pageSize, orderBy, cancellationToken);
    }
    public async Task<IPagedList<TTarget>> QueryPageAsync<TEntity, TTarget>(object whereObj, int pageIndex, int pageSize, string orderBy = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryPageImplAsync<TTarget>(entityType, targetType, whereObj, pageIndex, pageSize, orderBy, cancellationToken);
    }
    public async Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(string rawSql, int pageIndex, int pageSize, string orderBy = null, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var sql = this.BuildPageSql(rawSql, pageIndex, pageSize, orderBy);
        return await this.QueryPageImplWithAsync<TEntity>(sql, whereObj, cmdType, cancellationToken);
    }
    public async Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.QueryPageImplWithAsync<TEntity>(sql, whereObj, cmdType, cancellationToken);
    public async Task<IQueryReader> QueryMultipleAsync(string sql, object whereObj = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }
        return await this.QueryMultipleImplAsync(command, cancellationToken);
    }
    //public async Task<IQueryReader> QueryMultipleAsync(Action<IQueryBuilder> queryInitializer, CancellationToken cancellationToken = default)
    //{
    //    if (queryInitializer == null)
    //        throw new ArgumentNullException("queryInitializer");

    //    var command = this.connection.CreateCommand();
    //    command.Transaction = this.transaction;
    //    var builder = new QueryBuilder(this.dbFactory, this.connection, command);
    //    queryInitializer(builder);
    //    return await this.QueryMultipleImplAsync(builder.command, cancellationToken);
    //}
    #endregion

    #region 同步方法
    public TEntity Get<TEntity>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        return this.QueryFirstImpl<TEntity>(entityType, entityType, whereObj);
    }
    public TTarget Get<TEntity, TTarget>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return this.QueryFirstImpl<TTarget>(entityType, targetType, whereObj);
    }
    public int Create<TEntity>(object entity)
    {
        if (entity == null) throw new ArgumentNullException("entity");

        var entityType = typeof(TEntity);
        var parameterInfo = RepositoryHelper.CreateParameterInfo(entity);
        var createInfo = RepositoryHelper.BuildCreateCache(this.dbFactory, this.connection, entityType, parameterInfo);

        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        createInfo.Initializer(command, entity);

        this.connection.Open();
        if (parameterInfo.IsMulti) return command.ExecuteNonQuery();
        else
        {
            if (createInfo.IsAutoIncrement)
                return this.QueryFirstImpl<int>(command, entityType, typeof(int));
            else return command.ExecuteNonQuery();
        }
    }
    public int Update<TEntity>(object entity)
    {
        if (entity == null) throw new ArgumentNullException("entity");

        var entityType = typeof(TEntity);
        var parameterInfo = RepositoryHelper.CreateParameterInfo(entity);
        var commandInitializer = RepositoryHelper.BuildUpdateKeyCache(this.dbFactory, this.connection, entityType, parameterInfo);

        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, entity);

        this.connection.Open();
        return command.ExecuteNonQuery();
    }
    public int Update<TEntity>(object updateObj, object whereObj)
    {
        if (updateObj == null) throw new ArgumentNullException("updateObj");
        if (whereObj == null) throw new ArgumentNullException("whereObj");

        var entityType = typeof(TEntity);
        var updateObjInfo = RepositoryHelper.CreateParameterInfo(updateObj);
        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);

        if (updateObjInfo.IsMulti || whereObjInfo.IsMulti)
            throw new Exception("updateObj和whereObj参数暂时都不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

        var commandInitializer = RepositoryHelper.BuildUpdateCache(this.dbFactory, this.connection, entityType, updateObjInfo, whereObjInfo);
        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, updateObj, whereObj);

        this.connection.Open();
        return command.ExecuteNonQuery();
    }
    public int Delete<TEntity>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (whereObjInfo.IsMulti)
            throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");
        var commandInitializer = RepositoryHelper.BuildDeleteCache(this.dbFactory, this.connection, typeof(TEntity), whereObjInfo);

        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, whereObj);

        this.connection.Open();
        return command.ExecuteNonQuery();
    }
    public bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");

        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (whereObjInfo.IsMulti)
            throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsCache(this.dbFactory, this.connection, entityType, whereObjInfo);

        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, whereObj);

        connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        var targetType = typeof(long);
        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        bool result = false;
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            result = Convert.ToInt64(readerResult, CultureInfo.InvariantCulture) > 0;
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    public int Execute(string sql, object parameters = null, CommandType cmdType = CommandType.Text)
    {
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (parameters != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(parameters);
            if (parameterInfo.IsMulti)
                throw new Exception("parameters参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, parameters);
        }

        this.connection.Open();
        return command.ExecuteNonQuery();
    }
    #endregion

    #region 异步方法
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        return await this.QueryFirstImplAsync<TEntity>(entityType, entityType, whereObj, cancellationToken);
    }
    public async Task<TTarget> GetAsync<TEntity, TTarget>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");
        var entityType = typeof(TEntity);
        var targetType = typeof(TTarget);
        return await this.QueryFirstImplAsync<TTarget>(entityType, targetType, whereObj, cancellationToken);
    }
    public async Task<int> CreateAsync<TEntity>(object entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException("entity");

        var entityType = typeof(TEntity);
        var parameterInfo = RepositoryHelper.CreateParameterInfo(entity);
        var createInfo = RepositoryHelper.BuildCreateCache(this.dbFactory, this.connection, entityType, parameterInfo);

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");
        createInfo.Initializer(command, entity);

        await this.connection.OpenAsync(cancellationToken);
        if (parameterInfo.IsMulti) return await command.ExecuteNonQueryAsync(cancellationToken);
        else
        {
            if (createInfo.IsAutoIncrement)
                return await this.QueryFirstImplAsync<int>(command, entityType, typeof(int), cancellationToken);
            else return await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
    public async Task<int> UpdateAsync<TEntity>(object entity, CancellationToken cancellationToken = default)
    {
        if (entity == null) throw new ArgumentNullException("entity");

        var entityType = typeof(TEntity);
        var parameterInfo = RepositoryHelper.CreateParameterInfo(entity);
        var commandInitializer = RepositoryHelper.BuildUpdateKeyCache(this.dbFactory, this.connection, entityType, parameterInfo);

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer.Invoke(cmd, entity);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<int> UpdateAsync<TEntity>(object updateObj, object whereObj, CancellationToken cancellationToken = default)
    {
        if (updateObj == null) throw new ArgumentNullException("updateObj");
        if (whereObj == null) throw new ArgumentNullException("whereObj");

        var entityType = typeof(TEntity);
        var updateObjInfo = RepositoryHelper.CreateParameterInfo(updateObj);
        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (updateObjInfo.IsMulti || whereObjInfo.IsMulti)
            throw new Exception("updateObj和whereObj参数暂时都不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");
        var commandInitializer = RepositoryHelper.BuildUpdateCache(this.dbFactory, this.connection, entityType, updateObjInfo, whereObjInfo);

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer.Invoke(cmd, updateObj, whereObj);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<int> DeleteAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");

        var entityType = typeof(TEntity);
        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (whereObjInfo.IsMulti)
            throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");
        var commandInitializer = RepositoryHelper.BuildDeleteCache(this.dbFactory, this.connection, entityType, whereObjInfo);

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer.Invoke(cmd, whereObj);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    public async Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null) throw new ArgumentNullException("whereObj");

        var whereObjInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (whereObjInfo.IsMulti)
            throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");
        var entityType = typeof(TEntity);
        var commandInitializer = RepositoryHelper.BuildExistsCache(this.dbFactory, this.connection, entityType, whereObjInfo);

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer.Invoke(cmd, whereObj);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        var targetType = typeof(long);
        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        bool result = false;
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            result = Convert.ToInt64(readerResult, CultureInfo.InvariantCulture) > 0;
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    public async Task<int> ExecuteAsync(string sql, object parameters = null, CommandType cmdType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        var cmd = this.connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = cmdType;
        cmd.Transaction = this.transaction;

        if (parameters != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(parameters);
            if (parameterInfo.IsMulti)
                throw new Exception("parameters参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(cmd, parameters);
        }

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
    #endregion

    public ISqlExpression<TEntity> From<TEntity>()
    {
        var visitor = new SqlExpressionVisitor(this.dbFactory, this.connection.OrmProvider);
        return new SqlExpression<TEntity>(this.dbFactory, this.connection, visitor.From(typeof(TEntity)));
    }

    #region Transaction
    public void Begin()
    {
        this.connection.Open();
        this.transaction = this.connection.BeginTransaction();
    }
    public void Commit()
    {
        if (this.transaction != null)
            this.transaction.Commit();
        this.transaction = null;
    }
    public void Rollback()
    {
        if (this.transaction != null)
            this.transaction.Rollback();
        this.transaction = null;
    }
    public void Close()
    {
        if (this.connection != null)
        {
            try { this.connection.Close(); }
            catch { }
        }
    }
    public void Dispose()
    {
        if (this.connection != null)
        {
            try
            {
                this.connection.Close();
                this.connection.Dispose();
            }
            catch { }
        }
    }
    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        await this.connection.OpenAsync(cancellationToken);
        this.transaction = await this.connection.BeginTransactionAsync(cancellationToken);
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction != null)
        {
            if (this.transaction is DbTransaction dbTransaction)
                await dbTransaction.CommitAsync(cancellationToken);
            else throw new Exception("当前数据库驱动不支持异步操作");
            this.transaction = null;
        }
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.transaction != null)
        {
            if (this.transaction is DbTransaction dbTransaction)
                await dbTransaction.RollbackAsync(cancellationToken);
            else throw new Exception("当前数据库驱动不支持异步操作");
            this.transaction = null;
        }
    }
    public async Task CloseAsync()
    {
        if (this.connection != null)
        {
            try { await this.connection.DisposeAsync(); }
            catch { }
        }
    }
    public async Task DisposeAsync()
    {
        if (this.connection != null)
        {
            try
            {
                await this.connection.CloseAsync();
                await this.connection.DisposeAsync();
            }
            catch { }
        }
    }
    #endregion

    #region 同步私有方法       
    private TTarget QueryFirstImpl<TTarget>(Type entityType, Type targetType, object whereObj)
    {
        TTarget result = default;
        CommandInitializer commandInitializer = null;

        var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
        if (parameterInfo.IsMulti)
            throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

        commandInitializer = RepositoryHelper.BuildQueryCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);

        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, whereObj);

        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        connection.Open();
        var reader = command.ExecuteReader(behavior);
        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    private TTarget QueryFirstImplWith<TTarget>(string sql, Type entityType, Type targetType, object whereObj, CommandType cmdType)
    {
        TTarget result = default;
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;

        connection.Open();
        var reader = command.ExecuteReader(behavior);
        var readerFunc = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, reader, entityType, targetType);
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    private TTarget QueryFirstImpl<TTarget>(IDbCommand command, Type entityType, Type targetType)
    {
        TTarget result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        connection.Open();
        var reader = command.ExecuteReader(behavior);
        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    private List<TTarget> QueryImpl<TTarget>(Type entityType, Type targetType, object whereObj)
    {
        var result = new List<TTarget>();
        CommandInitializer commandInitializer = null;
        if (whereObj == null)
            commandInitializer = RepositoryHelper.BuildQuerySelectCache(this.dbFactory, this.connection, entityType, targetType);
        else
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            commandInitializer = RepositoryHelper.BuildQueryCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);
        }
        var command = connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, whereObj);

        var behavior = CommandBehavior.SequentialAccess;
        connection.Open();
        var reader = command.ExecuteReader(behavior);
        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result.Add((TTarget)readerResult);
            else result.Add((TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture));
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    private List<TTarget> QueryImplWith<TTarget>(string sql, Type entityType, Type targetType, object whereObj, CommandType cmdType)
    {
        var result = new List<TTarget>();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }

        connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        var readerFunc = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, reader, entityType, targetType);
        while (reader.Read())
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result.Add((TTarget)readerResult);
            else result.Add((TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture));
        }
        while (reader.NextResult()) { }
        reader.Close();
        reader.Dispose();
        return result;
    }
    private IPagedList<TTarget> QueryPageImpl<TTarget>(Type entityType, Type targetType, object whereObj, int pageIndex, int pageSize, string orderBy)
    {
        PagedCommandInitializer commandInitializer = null;
        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            commandInitializer = RepositoryHelper.BuildQueryPageCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);
        }
        else commandInitializer = RepositoryHelper.BuildQueryPageCache(this.dbFactory, this.connection, entityType, targetType);

        var command = this.connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, pageIndex, pageSize, orderBy, whereObj);

        var reader = this.QueryMultipleImpl(command);
        return reader.ReadPageList<TTarget>();
    }
    private IPagedList<TTarget> QueryPageImplWith<TTarget>(string sql, object whereObj, CommandType cmdType)
    {
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }

        var reader = this.QueryMultipleImpl(command);
        return reader.ReadPageList<TTarget>();
    }
    private IQueryReader QueryMultipleImpl(IDbCommand command)
    {
        this.connection.Open();
        var behavior = CommandBehavior.SequentialAccess;
        var reader = command.ExecuteReader(behavior);
        return new QueryReader(this.dbFactory, this.connection, command, reader);
    }
    private string BuildPageSql(string rawSql, int pageIndex, int pageSize, string orderBy = null)
    {
        if (pageIndex >= 1) pageIndex = pageIndex - 1;
        var skip = pageIndex * pageSize;

        var buidler = new StringBuilder();
        buidler.AppendFormat("SELECT COUNT(*) FROM({0}) _PagingTotal;", rawSql);
        if (rawSql.ToUpper().Contains("UNION"))
            buidler.AppendFormat("SELECT * FROM({0}) _PagingList", rawSql);
        else buidler.Append(rawSql);
        if (!String.IsNullOrEmpty(orderBy)) buidler.Append(" " + orderBy);
        buidler.AppendFormat(" LIMIT {0}", pageSize);
        buidler.AppendFormat(" OFFSET {0}", skip);
        return buidler.ToString();
    }
    #endregion

    #region 异步私有方法
    private async Task<TTarget> QueryFirstImplAsync<TTarget>(Type entityType, Type targetType, object whereObj, CancellationToken cancellationToken)
    {
        TTarget result = default;
        CommandInitializer commandInitializer = null;
        if (whereObj == null)
            commandInitializer = RepositoryHelper.BuildQuerySelectCache(this.dbFactory, this.connection, entityType, targetType);
        else
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            commandInitializer = RepositoryHelper.BuildQueryCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);
        }

        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer?.Invoke(cmd, whereObj);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    private async Task<TTarget> QueryFirstImplWithAsync<TTarget>(string sql, Type entityType, Type targetType, object whereObj, CommandType cmdType, CancellationToken cancellationToken)
    {
        TTarget result = default;
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = cmdType;
        cmd.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(cmd, whereObj);
        }

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        var readerFunc = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, reader, entityType, targetType);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    private async Task<TTarget> QueryFirstImplAsync<TTarget>(IDbCommand cmd, Type entityType, Type targetType, CancellationToken cancellationToken)
    {
        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        TTarget result = default;
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);

        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result = (TTarget)readerResult;
            else result = (TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture);
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    private async Task<List<TTarget>> QueryImplAsync<TTarget>(Type entityType, Type targetType, object whereObj, CancellationToken cancellationToken)
    {
        var result = new List<TTarget>();
        CommandInitializer commandInitializer = null;
        if (whereObj == null)
            commandInitializer = RepositoryHelper.BuildQuerySelectCache(this.dbFactory, this.connection, entityType, targetType);
        else
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            commandInitializer = RepositoryHelper.BuildQueryCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);
        }
        var cmd = connection.CreateCommand();
        cmd.Transaction = this.transaction;
        commandInitializer?.Invoke(cmd, whereObj);

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);

        var readerFunc = RepositoryHelper.GetReader(true, this.dbFactory, this.connection, reader, entityType, targetType);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result.Add((TTarget)readerResult);
            else result.Add((TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture));
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    private async Task<List<TTarget>> QueryImplWithAsync<TTarget>(string sql, Type entityType, Type targetType, object whereObj, CommandType cmdType, CancellationToken cancellationToken)
    {
        var result = new List<TTarget>();
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = cmdType;
        cmd.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(cmd, whereObj);
        }

        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);

        var readerFunc = RepositoryHelper.GetReader(false, this.dbFactory, this.connection, reader, entityType, targetType);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readerResult = readerFunc(reader);
            if (readerResult is TTarget) result.Add((TTarget)readerResult);
            else result.Add((TTarget)Convert.ChangeType(readerResult, targetType, CultureInfo.InvariantCulture));
        }
        while (await reader.NextResultAsync(cancellationToken)) { }
        await reader.CloseAsync();
        await reader.DisposeAsync();
        return result;
    }
    private async Task<IPagedList<TTarget>> QueryPageImplAsync<TTarget>(Type entityType, Type targetType, object whereObj, int pageIndex, int pageSize, string orderBy, CancellationToken cancellationToken)
    {
        PagedCommandInitializer commandInitializer = null;
        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            commandInitializer = RepositoryHelper.BuildQueryPageCache(this.dbFactory, this.connection, entityType, targetType, parameterInfo);
        }
        else commandInitializer = RepositoryHelper.BuildQueryPageCache(this.dbFactory, this.connection, entityType, targetType);

        var command = this.connection.CreateCommand();
        command.Transaction = this.transaction;
        commandInitializer.Invoke(command, pageIndex, pageSize, orderBy, whereObj);

        var reader = await this.QueryMultipleImplAsync(command, cancellationToken);
        return await reader.ReadPageListAsync<TTarget>(cancellationToken);
    }
    private async Task<IPagedList<TTarget>> QueryPageImplWithAsync<TTarget>(string sql, object whereObj, CommandType cmdType, CancellationToken cancellationToken)
    {
        var command = this.connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = cmdType;
        command.Transaction = this.transaction;

        if (whereObj != null)
        {
            var parameterInfo = RepositoryHelper.CreateParameterInfo(whereObj);
            if (parameterInfo.IsMulti)
                throw new Exception("whereObj参数暂时不支持IEnumerable类型，参数的属性值可以是IEnumerable类型");

            var commandInitializer = RepositoryHelper.BuildQueryWhereSqlCache(this.dbFactory, this.connection, cmdType, parameterInfo);
            commandInitializer.Invoke(command, whereObj);
        }

        var reader = await this.QueryMultipleImplAsync(command, cancellationToken);
        return await reader.ReadPageListAsync<TTarget>(cancellationToken);
    }
    private async Task<IQueryReader> QueryMultipleImplAsync(IDbCommand cmd, CancellationToken cancellationToken)
    {
        if (!(cmd is DbCommand command))
            throw new Exception("当前数据库驱动不支持异步SQL查询");

        await this.connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        return new QueryReader(this.dbFactory, this.connection, command, reader);
    }
    #endregion
}
