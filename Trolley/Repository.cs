using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Threading.Tasks;

namespace Trolley
{
    public class Repository : IRepository
    {
        #region 属性
        protected DbConnection Connection { get; private set; }
        protected DbTransaction Transaction { get; private set; }
        public string ConnString { get; private set; }
        public IOrmProvider Provider { get; private set; }
        #endregion

        #region 构造方法
        public Repository()
        {
            this.ConnString = OrmProviderFactory.DefaultConnString;
            this.Provider = OrmProviderFactory.DefaultProvider;
        }
        public Repository(string connString)
        {
            this.ConnString = connString;
            this.Provider = OrmProviderFactory.GetProvider(connString);
        }
        public Repository(string connString, DbTransaction transaction)
        {
            this.ConnString = connString;
            this.Provider = OrmProviderFactory.GetProvider(connString);
            if (transaction != null)
            {
                this.Transaction = transaction;
                this.Connection = transaction.Connection;
            }
        }
        #endregion

        #region 同步方法
        public TEntity QueryFirst<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryFirstImpl<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public List<TEntity> Query<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryImpl<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public PagedList<TEntity> QueryPage<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql + orderBy ?? "");
            sql = RepositoryHelper.GetPagingCache(cacheKey, this.ConnString, sql, pageIndex, pageSize, orderBy, this.Provider);
            return this.QueryPageImpl<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public int ExecSql(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.ExecSqlImpl(cacheKey, sql, cmdType, objParameter);
        }
        #endregion

        #region 异步方法
        public async Task<TEntity> QueryFirstAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryFirstImplAsync<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public async Task<List<TEntity>> QueryAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryImplAsync<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public async Task<PagedList<TEntity>> QueryPageAsync<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryPageImplAsync<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter);
        }
        public async Task<int> ExecSqlAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.ExecSqlImplAsync(cacheKey, sql, cmdType, objParameter);
        }
        #endregion

        #region 实现IDisposable
        public void Dispose()
        {
            if (this.Transaction != null) this.Transaction.Dispose();
            if (this.Connection != null) this.Connection.Dispose();
        }
        #endregion

        #region 私有方法
        private void Open()
        {
            if (this.Connection.State == ConnectionState.Broken) this.Connection.Close();
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();
        }
        private TEntity QueryFirstImpl<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            TEntity result = default(TEntity);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null || funcResult is TEntity) result = (TEntity)funcResult;
                        else result = (TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture);
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null || funcResult is TEntity) result = (TEntity)funcResult;
                    else result = (TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture);
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private List<TEntity> QueryImpl<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            List<TEntity> result = new List<TEntity>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TEntity) result.Add((TEntity)funcResult);
                        else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TEntity) result.Add((TEntity)funcResult);
                    else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private PagedList<TEntity> QueryPageImpl<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            PagedList<TEntity> result = new PagedList<TEntity>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TEntity) result.Add((TEntity)funcResult);
                        else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TEntity) result.Add((TEntity)funcResult);
                    else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private int ExecSqlImpl(int hashKey, string sql, CommandType cmdType = CommandType.Text, object objParameter = null)
        {
            DbCommand command = null;
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = cmdType;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                result = command.ExecuteNonQuery();
            }
            return result;
        }
        private async Task<TEntity> QueryFirstImplAsync<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            TEntity result = default(TEntity);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null || funcResult is TEntity) result = (TEntity)funcResult;
                        else result = (TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture);
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null || funcResult is TEntity) result = (TEntity)funcResult;
                    else result = (TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture);
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<List<TEntity>> QueryImplAsync<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            List<TEntity> result = new List<TEntity>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TEntity) result.Add((TEntity)funcResult);
                        else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TEntity) result.Add((TEntity)funcResult);
                    else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<PagedList<TEntity>> QueryPageImplAsync<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            PagedList<TEntity> result = new PagedList<TEntity>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TEntity) result.Add((TEntity)funcResult);
                        else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, entityType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TEntity) result.Add((TEntity)funcResult);
                    else result.Add((TEntity)Convert.ChangeType(funcResult, entityType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, CommandType cmdType = CommandType.Text, object objParameter = null)
        {
            DbCommand command = null;
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    result = await command.ExecuteNonQueryAsync();
                    conn.Close();
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = cmdType;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, objParameter.GetType(), this.Provider);
                    paramAction(command, objParameter);
                }
                this.Open();
                result = await command.ExecuteNonQueryAsync();
            }
            return result;
        }
        #endregion
    }
}
