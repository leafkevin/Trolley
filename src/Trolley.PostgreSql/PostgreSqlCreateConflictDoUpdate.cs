using System;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlCreateConflictDoUpdate<TEntity> : PostgreSqlIdentitiedCreated, IPostgreSqlCreateConflictDoUpdate<TEntity>
{
    private PostgreSqlCreateVisitor dialectVisitor;
    private StringBuilder builder => this.dialectVisitor.UpdateBuilder;
    private bool isUseKeysOrConstraint = false;

    public PostgreSqlCreateConflictDoUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as PostgreSqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new(" ON CONFLICT");
    }

    #region DoNothing
    public virtual IIdentitiedCreated DoNothing()
    {
        this.builder.Append(" DO NOTHING");
        return this;
    }
    #endregion

    #region UseKeys
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> UseKeys()
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        this.builder.Append(" (");
        var keyMappers = this.Visitor.Tables[0].Mapper.KeyMembers;
        if (keyMappers.Count > 1)
        {
            for (int i = 0; i < keyMappers.Count; i++)
            {
                if (i > 0) this.builder.Append(',');
                var keyMapper = keyMappers[i];
                this.builder.Append(this.OrmProvider.GetFieldName(keyMapper.FieldName));
            }
        }
        else this.builder.Append(this.OrmProvider.GetFieldName(keyMappers[0].FieldName));
        this.builder.Append(") DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
        return this;
    }
    #endregion

    #region UseConstraint
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName)
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        if (string.IsNullOrEmpty(constraintName))
            throw new ArgumentNullException("参数constraintName不可为null");
        this.builder.Append($" ON CONSTRAINT {constraintName} DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
        return this;
    }
    #endregion

    #region Set
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));

        this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsAssignment });
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (!condition) return this;
        this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsAssignment });
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValueSelector == null)
            throw new ArgumentNullException(nameof(fieldValueSelector));

        this.dialectVisitor.VisitSetFieldExpression(fieldSelector, fieldValueSelector);
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
    {
        if (!condition) return this;
        this.Set(fieldSelector, fieldValueSelector);
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.dialectVisitor.VisitWithSetField(fieldSelector, fieldValue);
        return this;
    }
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (!condition) return this;
        this.Set(fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region Where
    public virtual IPostgreSqlCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var sqlSegment = this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = predicate });
        this.builder.Append($" WHERE {sqlSegment.Body}");
        return this;
    }
    #endregion

    #region Returning
    public virtual IResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        this.dialectVisitor.Returning(fieldsSelector);
        return this.OrmProvider.NewResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}
public class PostgreSqlBulkCreateConflictDoUpdate<TEntity> : PostgreSqlCreated, IPostgreSqlBulkCreateConflictDoUpdate<TEntity>
{
    private PostgreSqlCreateVisitor dialectVisitor;
    private StringBuilder builder => this.dialectVisitor.UpdateBuilder;
    private bool isUseKeysOrConstraint = false;

    public PostgreSqlBulkCreateConflictDoUpdate(DbContext dbContext, ICreateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.dialectVisitor = visitor as PostgreSqlCreateVisitor;
        this.dialectVisitor.UpdateBuilder = new(" ON CONFLICT");
    }

    #region DoNothing
    public virtual ICreated DoNothing()
    {
        this.builder.Append(" DO NOTHING");
        return this;
    }
    #endregion

    #region UseKeys
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseKeys()
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        this.builder.Append(" (");
        var keyMappers = this.Visitor.Tables[0].Mapper.KeyMembers;
        if (keyMappers.Count > 1)
        {
            for (int i = 0; i < keyMappers.Count; i++)
            {
                if (i > 0) this.builder.Append(',');
                var keyMapper = keyMappers[i];
                this.builder.Append(this.OrmProvider.GetFieldName(keyMapper.FieldName));
            }
        }
        else this.builder.Append(this.OrmProvider.GetFieldName(keyMappers[0].FieldName));
        this.builder.Append(") DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
        return this;
    }
    #endregion

    #region UseConstraint
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> UseConstraint(string constraintName)
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        if (string.IsNullOrEmpty(constraintName))
            throw new ArgumentNullException("参数constraintName不可为null");
        this.builder.Append($" ON CONSTRAINT {constraintName} DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
        return this;
    }
    #endregion

    #region Set
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));

        this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsAssignment });
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (!condition) return this;
        this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsAssignment });
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValueSelector == null)
            throw new ArgumentNullException(nameof(fieldValueSelector));

        this.dialectVisitor.VisitSetFieldExpression(fieldSelector, fieldValueSelector);
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
    {
        if (!condition) return this;
        this.Set(fieldSelector, fieldValueSelector);
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        this.dialectVisitor.VisitWithSetField(fieldSelector, fieldValue);
        return this;
    }
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (!condition) return this;
        this.Set(fieldSelector, fieldValue);
        return this;
    }
    #endregion

    #region Where
    public virtual IPostgreSqlBulkCreateConflictDoUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var sqlSegment = this.dialectVisitor.VisitAndDeferred(new SqlFieldSegment { Expression = predicate });
        this.builder.Append($" WHERE {sqlSegment.Body}");
        return this;
    }
    #endregion

    #region Returning
    public virtual IBulkResultCommand<TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector)
    {
        if (fieldsSelector == null)
            throw new ArgumentNullException(nameof(fieldsSelector));
        this.dialectVisitor.Returning(fieldsSelector);
        return this.OrmProvider.NewBulkResultCreated<TResult>(this.DbContext, this.Visitor);
    }
    #endregion
}