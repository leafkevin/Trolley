using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerContinuedUpdate<TEntity> : ContinuedUpdate<TEntity>, ISqlServerContinuedUpdate<TEntity>
{
    #region Properties
    public SqlServerUpdateVisitor DialectVisitor { get; private set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public SqlServerContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqlServerUpdateVisitor;
    }
    #endregion

    #region Set
    public override ISqlServerContinuedUpdate<TEntity> Set<TUpdateObj>(TUpdateObj updateObj)
    {
        base.Set(updateObj);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
    {
        base.Set(condition, updateObj);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.Set(fieldSelector, fieldValue);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        base.Set(condition, fieldSelector, fieldValue);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        base.Set(fieldsAssignment);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        base.Set(condition, fieldsAssignment);
        return this;
    }
    #endregion

    #region SetFrom
    public override ISqlServerContinuedUpdate<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        base.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IQuery<TField>>> valueSelector)
    {
        base.SetFrom(condition, fieldSelector, valueSelector);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        base.SetFrom(fieldsAssignment);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        base.SetFrom(condition, fieldsAssignment);
        return this;
    }
    #endregion

    #region IgnoreFields
    public override ISqlServerContinuedUpdate<TEntity> IgnoreFields(params string[] fieldNames)
    {
        base.IgnoreFields(fieldNames);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.IgnoreFields(fieldsSelector);
        return this;
    }
    #endregion

    #region OnlyFields
    public override ISqlServerContinuedUpdate<TEntity> OnlyFields(params string[] fieldNames)
    {
        base.OnlyFields(fieldNames);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector)
    {
        base.OnlyFields(fieldsSelector);
        return this;
    }
    #endregion

    #region Where/And
    public override ISqlServerUpdated<TEntity> Where<TWhereObj>(TWhereObj whereObj)
    {
        base.Where(whereObj);
        return this.OrmProvider.NewUpdated<TEntity>(this.DbContext, this.Visitor) as ISqlServerUpdated<TEntity>;
    }
    public override ISqlServerContinuedUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        base.Where(predicate);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        base.Where(condition, ifPredicate, elsePredicate);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> And(Expression<Func<TEntity, bool>> predicate)
    {
        base.And(predicate);
        return this;
    }
    public override ISqlServerContinuedUpdate<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        base.And(condition, ifPredicate, elsePredicate);
        return this;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql = null;
        dbParameters = null;
        var builder = new StringBuilder();
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            var entityType = this.Visitor.Tables[0].EntityType;
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var tableName = this.Visitor.OrmProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");
            var memberMappers = this.Visitor.GetRefMemberMappers(entityType, fromMapper);
            //添加临时表           
            builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
            var pkColumns = new List<string>();
            foreach (var memberMapper in memberMappers)
            {
                var refMemberMapper = memberMapper.RefMemberMapper;
                var fieldName = this.Visitor.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                if (refMemberMapper.IsKey)
                {
                    builder.Append(" NOT NULL");
                    pkColumns.Add(fieldName);
                }
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
            builder.AppendLine(");");
            if (this.Visitor.IsNeedFetchShardingTables)
                builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));

            Action<StringBuilder, string> sqlExecutor = (builder, tableName) =>
            {
                builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(tableName)} a INNER JOIN {tableName} b ON ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
                builder.Append(" SET ");
                int setIndex = 0;
                for (int i = 0; i < memberMappers.Count; i++)
                {
                    var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"a.{fieldName}=b.{fieldName}");
                    setIndex++;
                }
            };
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                int index = 0;
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                foreach (var shardingTableName in tableNames)
                {
                    if (index > 0) builder.Append(';');
                    sqlExecutor.Invoke(builder, shardingTableName);
                }
            }
            else sqlExecutor.Invoke(builder, this.Visitor.Tables[0].Body ?? fromMapper.TableName);
            builder.Append($"DROP TABLE {tableName}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));
                builder.Append(';');
            }
            using var command = this.DbContext.CreateCommand();
            sql = this.Visitor.BuildCommand(this.DbContext, command);
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(sql);
                sql = builder.ToString();
            }
            dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        }
        builder.Clear();
        builder = null;
        return sql;
    }
    #endregion
}
