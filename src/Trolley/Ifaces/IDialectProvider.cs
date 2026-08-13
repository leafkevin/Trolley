using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IDialectProvider
{
    DbContext DbContext { get; set; }

    #region UseMasterCommand/UseSlaveCommand
    (bool, ITheaConnection, ITheaCommand) UseMasterCommand(ICommandVisitor commandContext = null);
    (bool, ITheaConnection, ITheaCommand) UseSlaveCommand(ICommandVisitor commandContext = null);
    #endregion

    #region QueryScalar
    TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text);
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text);
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    TResult QueryScalar<TResult>(IQueryVisitor visitor);
    Task<TResult> QueryScalarAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default);
    bool QueryExists(IQueryVisitor visitor);
    Task<bool> QueryExistsAsync(IQueryVisitor visitor, CancellationToken cancellationToken = default);
    #endregion

    #region QueryValue
    List<TTarget> QueryRaw<TTarget>(string rawSql, Func<ITheaDataReader, List<TTarget>> readerInitializer, CommandType commandType = CommandType.Text);
    Task<List<TTarget>> QueryRawAsync<TTarget>(string rawSql, Func<ITheaDataReader, CancellationToken, Task<List<TTarget>>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    List<TTarget> QueryRaw<TTarget>(string rawSql, object parameters, Func<ITheaDataReader, List<TTarget>> readerInitializer, CommandType commandType = CommandType.Text);
    Task<List<TTarget>> QueryRawAsync<TTarget>(string rawSql, object parameters, Func<ITheaDataReader, CancellationToken, Task<List<TTarget>>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    List<TTarget> QueryRaw<TTarget>(string rawSql, List<IDbDataParameter> parameters, Func<ITheaDataReader, List<TTarget>> readerInitializer, CommandType commandType = CommandType.Text);
    Task<List<TTarget>> QueryRawAsync<TTarget>(string rawSql, List<IDbDataParameter> parameters, Func<ITheaDataReader, CancellationToken, Task<List<TTarget>>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    #region Query
    TResult Query<TTarget, TResult>(string rawSql, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text);
    Task<TResult> QueryAsync<TTarget, TResult>(string rawSql, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    TResult Query<TTarget, TResult>(string rawSql, bool isBulk, object parameters, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text);
    Task<TResult> QueryAsync<TTarget, TResult>(string rawSql, bool isBulk, object parameters, Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    TResult QueryRaw<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer, CommandType commandType = CommandType.Text);
    Task<TResult> QueryRawAsync<TTarget, TResult>(string rawSql, bool isBulk, List<IDbDataParameter> parameters, Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);

    TResult Query<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, TResult> readerInitializer);
    Task<TResult> QueryAsync<TEntity, TResult>(object whereObjs, bool isUseKey, bool isBulk, Func<ITheaDataReader, Func<ITheaDataReader, object>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default);
    #endregion

    #region QueryVisitor
    TResult QueryFrom<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<ReaderField>, TResult> readerInitializer);
    Task<TResult> QueryFromAsync<TEntity, TResult>(IQueryVisitor visitor, bool isBulk, Func<Type, ITheaDataReader, List<ReaderField>, CancellationToken, Task<TResult>> readerInitializer, CancellationToken cancellationToken = default);
    IPagedList<TResult> QueryPage<TResult>(IQueryVisitor visitor);
    Task<IPagedList<TResult>> QueryPageAsync<TResult>(IQueryVisitor visitor, CancellationToken cancellationToken = default);
    #endregion

    #region Exists
    bool Exists<TEntity>(object whereObj, bool isUseKey, bool isBulk);
    Task<bool> ExistsAsync<TEntity>(object whereObj, bool isUseKey, bool isBulk, CancellationToken cancellationToken = default);

    #endregion

    #region Create
    int Create<TEntity>(object insertObj);
    Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
    int Create<TEntity>(IEnumerable insertObjs, int bulkCount);
    Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken = default);
    TResult CreateIdentity<TEntity, TResult>(object insertObj);
    Task<TResult> CreateIdentityAsync<TEntity, TResult>(object insertObj, CancellationToken cancellationToken = default);

    TResult CreateIdentity<TResult>(ICreateVisitor visitor);
    Task<TResult> CreateIdentityAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default);

    TResult CreateResult<TTarget, TResult>(ICreateVisitor visitor, Func<ITheaDataReader, List<ReaderField>, Func<ITheaDataReader, List<ReaderField>, object>, TResult> readerInitializer);
    Task<TResult> CreateResultAsync<TTarget, TResult>(ICreateVisitor visitor, Func<ITheaDataReader, List<ReaderField>, Func<ITheaDataReader, List<ReaderField>, object>, TResult> readerInitializer, CancellationToken cancellationToken = default);
    #endregion

    #region Update
    int Update<TEntity>(object updateObj);
    Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default);
    int Update<TEntity>(IEnumerable updateObjs, int bulkCount);
    Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default);
    #endregion

    #region Delete
    int Delete<TEntity>(object whereObjs, bool isUseKey, bool isBulk);
    Task<int> DeleteAsync<TEntity>(object whereObjs, bool isUseKey, bool isBulk, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text);
    Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    int Execute(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    Task<int> ExecuteAsync(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    #region Others   
    void BeginTransaction();
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken = default);
    #endregion

    #region BuildSql
    (string, List<ReaderField>) BuildSql(IQueryVisitor visitor);
    string BuildShardingTablesSqlByFormat(SqlVisitor visitor, string formatSql, string jointMark);
    string GetShardingTable(Type entityType, params object[] fieldValues);
    #endregion
}