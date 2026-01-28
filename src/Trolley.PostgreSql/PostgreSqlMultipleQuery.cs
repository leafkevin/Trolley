using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlMultipleQuery : MultipleQuery
{
    #region Constructor
    public PostgreSqlMultipleQuery(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region From
    public new IPostgreSqlQuery<T> From<T>(char tableAsStart = 'a')
        => base.From<T>(tableAsStart) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
        => base.From<T1, T2>(tableAsStart) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
        => base.From<T1, T2, T3>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region From SubQuery  
    public new IPostgreSqlQuery<T> FromQuery<T>(IQuery<T> subQuery)
        => base.FromQuery(subQuery) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
        => base.FromQuery(subQueryExpr) as IPostgreSqlQuery<T>;
    #endregion

    #region GetShardingTableNames
    public override IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        this.AddReader(typeof(string), sql, false);
        return this;
    }
    #endregion
}