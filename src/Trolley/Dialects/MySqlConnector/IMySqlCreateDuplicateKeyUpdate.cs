using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlCreateDuplicateKeyUpdate<TEntity>
{
    #region Values
    TField Values<TField>(TField fieldSelector);
    #endregion

    #region Alias
    IMySqlCreateDuplicateKeyUpdate<TEntity> Alias(string aliasName = "row");
    #endregion

    #region Set
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    IMySqlCreateDuplicateKeyUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion
}