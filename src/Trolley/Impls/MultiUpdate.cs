using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

class MultiUpdate<TEntity> : IMultiUpdate<TEntity>
{
    #region Fields
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private readonly bool isParameterized;
    #endregion

    #region Constructor
    public MultiUpdate(MultipleQuery multiQuery)
    {
        this.multiQuery = multiQuery;
        this.connection = multiQuery.Connection;
        this.ormProvider = multiQuery.OrmProvider;
        this.mapProvider = multiQuery.MapProvider;
        this.isParameterized = multiQuery.IsParameterized;
    }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.Set(fieldsAssignment);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }
    public IMultiUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.Set(fieldSelector, fieldValue);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }

    public IMultiUpdateSetting<TEntity> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateSetting<TEntity> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.SetRaw(rawSql, updateObj);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }
    public IMultiUpdateSetting<TEntity> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateSetting<TEntity> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.SetWith(null, updateObj);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }
    public IMultiUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }

    public IMultiUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.SetFrom(fieldsAssignment);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }
    public IMultiUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}");
        if (condition) visitor.SetFrom(fieldSelector, valueSelector);
        return new MultiUpdateSetting<TEntity>(this.multiQuery, visitor);
    }
    #endregion

    #region WithBulk
    public IMultiUpdateSet<TEntity> WithBulk(IEnumerable updateObjs)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .SetBulkFirst(null, updateObjs);
        return new MultiUpdateSet<TEntity>(this.multiQuery, visitor, updateObjs);
    }
    public IMultiUpdateSet<TEntity> WithBulk<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, IEnumerable updateObjs)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持New或MemberInit类型表达式");

        Type updateObjType = null;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            if (!updateObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的updateObjs元素类型，updateObjs元素类型可以是字典或是实体类或是多字段元组");
            break;
        }
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .SetBulkFirst(fieldsSelectorOrAssignment, updateObjType);
        return new MultiUpdateSet<TEntity>(this.multiQuery, visitor, updateObjs);
    }
    #endregion

    #region From
    public IMultiUpdateFrom<TEntity, TSource> From<TSource>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .From(typeof(TSource));
        return new MultiUpdateFrom<TEntity, TSource>(this.multiQuery, visitor);
    }
    public IMultiUpdateFrom<TEntity, T1, T2> From<T1, T2>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .From(typeof(T1), typeof(T2));
        return new MultiUpdateFrom<TEntity, T1, T2>(this.multiQuery, visitor);
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .From(typeof(T1), typeof(T2), typeof(T3));
        return new MultiUpdateFrom<TEntity, T1, T2, T3>(this.multiQuery, visitor);
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return new MultiUpdateFrom<TEntity, T1, T2, T3, T4>(this.multiQuery, visitor);
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .From(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return new MultiUpdateFrom<TEntity, T1, T2, T3, T4, T5>(this.multiQuery, visitor);
    }
    #endregion

    #region Join
    public IMultiUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
            .Join("INNER JOIN", typeof(TSource), joinOn);
        return new MultiUpdateJoin<TEntity, TSource>(this.multiQuery, visitor);
    }
    public IMultiUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn)
    {
        var visitor = this.ormProvider.NewUpdateVisitor(this.connection.DbKey, this.mapProvider, typeof(TEntity), this.isParameterized, multiParameterPrefix: $"m{this.multiQuery.ReaderAfters.Count}")
           .Join("INNER JOIN", typeof(TSource), joinOn);
        return new MultiUpdateJoin<TEntity, TSource>(this.multiQuery, visitor);
    }
    #endregion
}
class MultiUpdateSet<TEntity> : IMultiUpdateSet<TEntity>
{
    #region Fields
    private readonly MultipleQuery multiQuery;
    private readonly TheaConnection connection;
    private readonly IDbCommand command;
    private readonly IUpdateVisitor visitor;
    private readonly IEnumerable updateObjs;
    #endregion

    #region Constructor
    public MultiUpdateSet(MultipleQuery multiQuery, IUpdateVisitor visitor, IEnumerable updateObjs)
    {
        this.multiQuery = multiQuery;
        this.visitor = visitor;
        this.command = multiQuery.Command;
        this.updateObjs = updateObjs;
    }
    #endregion

    #region Execute
    public IMultipleQuery Execute()
    {
        int index = 0;
        var builder = new StringBuilder();
        foreach (var updateObj in this.updateObjs)
        {
            if (index > 0) builder.Append(';');
            this.visitor.SetBulk(builder, this.command, updateObj, index);
            index++;
        }
        var sql = builder.ToString();
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        string sql = null;
        int index = 0;
        var builder = new StringBuilder();
        using var command = this.connection.CreateCommand();
        foreach (var updateObj in this.updateObjs)
        {
            if (index > 0) builder.Append(';');
            this.visitor.SetBulk(builder, command, updateObj, index);
            index++;
        }
        if (index > 0)
            sql = builder.ToString();
        if (command.Parameters != null && command.Parameters.Count > 0)
            dbParameters = command.Parameters.Cast<IDbDataParameter>().ToList();
        command.Dispose();
        return sql;
    }
    #endregion
}
class MultiUpdateBase
{
    #region Fields    
    protected readonly MultipleQuery multiQuery;
    protected readonly IDbCommand command;
    protected readonly IUpdateVisitor visitor;
    protected bool hasWhere = false;
    #endregion

    #region Constructor
    public MultiUpdateBase(MultipleQuery multiQuery, IUpdateVisitor visitor)
    {
        this.multiQuery = multiQuery;
        this.visitor = visitor;
        this.command = multiQuery.Command;
    }
    #endregion

    #region Execute
    public IMultipleQuery Execute()
    {
        if (!hasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        var sql = this.visitor.BuildSql(out var dbParameters);
        if (dbParameters != null && dbParameters.Count > 0)
            dbParameters.ForEach(f => this.command.Parameters.Add(f));
        Func<IDataReader, object> readerGetter = reader => reader.To<int>();
        this.multiQuery.AddReader(sql, readerGetter);
        return this.multiQuery;
    }
    #endregion

    #region ToSql
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        if (!hasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        return this.visitor.BuildSql(out dbParameters);
    }
    #endregion
}
class MultiUpdateSetting<TEntity> : MultiUpdateBase, IMultiUpdateSetting<TEntity>
{
    #region Constructor
    public MultiUpdateSetting(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateSetting<TEntity> Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
       => this.Set(true, fieldsAssignment);
    public IMultiUpdateSetting<TEntity> Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateSetting<TEntity> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateSetting<TEntity> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateSetting<TEntity> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateSetting<TEntity> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateSetting<TEntity> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateSetting<TEntity> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateSetting<TEntity> SetWith<TFields>(Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateSetting<TEntity> SetWith<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateSetting<TEntity> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateSetting<TEntity> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition) this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateSetting<TEntity> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateSetting<TEntity> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition) this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateSet<TEntity> Where<TFields>(TFields whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        this.visitor.WhereWith(whereObj);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateSetting<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateSetting<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateSetting<TEntity> And(Expression<Func<TEntity, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateSetting<TEntity> And(bool condition, Expression<Func<TEntity, bool>> ifPredicate, Expression<Func<TEntity, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateFrom<TEntity, T1> : MultiUpdateBase, IMultiUpdateFrom<TEntity, T1>
{
    #region Constructor
    public MultiUpdateFrom(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateFrom<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateFrom<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateFrom<TEntity, T1> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateFrom<TEntity, T1> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateFrom<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateFrom<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateFrom<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateFrom<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateFrom<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateFrom<TEntity, T1, T2> : MultiUpdateBase, IMultiUpdateFrom<TEntity, T1, T2>
{
    #region Constructor
    public MultiUpdateFrom(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateFrom<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateFrom<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateFrom<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateFrom<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateFrom<TEntity, T1, T2, T3> : MultiUpdateBase, IMultiUpdateFrom<TEntity, T1, T2, T3>
{
    #region Constructor
    public MultiUpdateFrom(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateFrom<TEntity, T1, T2, T3, T4> : MultiUpdateBase, IMultiUpdateFrom<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public MultiUpdateFrom(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> : MultiUpdateBase, IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public MultiUpdateFrom(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetRaw(string rawSql, object updateObj)
        => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateFrom<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateJoin<TEntity, T1> : MultiUpdateBase, IMultiUpdateJoin<TEntity, T1>
{
    #region Constructor
    public MultiUpdateJoin(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Join
    public IMultiUpdateJoin<TEntity, T1, T2> InnerJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T2), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2>(this.multiQuery, this.visitor);
    }
    public IMultiUpdateJoin<TEntity, T1, T2> LeftJoin<T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T2), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateJoin<TEntity, T1> Set<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1> Set<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateJoin<TEntity, T1> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1> SetRaw(string rawSql, object updateObj)
       => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateJoin<TEntity, T1> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateJoin<TEntity, T1> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1> SetWith<TFields>(Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateJoin<TEntity, T1> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateJoin<TEntity, T1> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateJoin<TEntity, T1> Where(Expression<Func<TEntity, T1, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateJoin<TEntity, T1> Where(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1> And(Expression<Func<TEntity, T1, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateJoin<TEntity, T1> And(bool condition, Expression<Func<TEntity, T1, bool>> ifPredicate, Expression<Func<TEntity, T1, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateJoin<TEntity, T1, T2> : MultiUpdateBase, IMultiUpdateJoin<TEntity, T1, T2>
{
    #region Constructor
    public MultiUpdateJoin(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Join
    public IMultiUpdateJoin<TEntity, T1, T2, T3> InnerJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T3), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3>(this.multiQuery, this.visitor);
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3> LeftJoin<T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T3), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateJoin<TEntity, T1, T2> Set<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateJoin<TEntity, T1, T2> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2> SetRaw(string rawSql, object updateObj)
       => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2> SetWith<TFields>(Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateJoin<TEntity, T1, T2> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateJoin<TEntity, T1, T2> Where(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2> Where(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2> And(Expression<Func<TEntity, T1, T2, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2> And(bool condition, Expression<Func<TEntity, T1, T2, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateJoin<TEntity, T1, T2, T3> : MultiUpdateBase, IMultiUpdateJoin<TEntity, T1, T2, T3>
{
    #region Constructor
    public MultiUpdateJoin(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Join
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> InnerJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T4), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3, T4>(this.multiQuery, this.visitor);
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> LeftJoin<T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T4), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3, T4>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetRaw(string rawSql, object updateObj)
       => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Where(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3> And(Expression<Func<TEntity, T1, T2, T3, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3> And(bool condition, Expression<Func<TEntity, T1, T2, T3, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateJoin<TEntity, T1, T2, T3, T4> : MultiUpdateBase, IMultiUpdateJoin<TEntity, T1, T2, T3, T4>
{
    #region Constructor
    public MultiUpdateJoin(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Join
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> InnerJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("INNER JOIN", typeof(T5), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.multiQuery, this.visitor);
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> LeftJoin<T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
        if (joinOn == null)
            throw new ArgumentNullException(nameof(joinOn));

        this.visitor.Join("LEFT JOIN", typeof(T5), joinOn);
        return new MultiUpdateJoin<TEntity, T1, T2, T3, T4, T5>(this.multiQuery, this.visitor);
    }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetRaw(string rawSql, object updateObj)
       => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Where(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> And(Expression<Func<TEntity, T1, T2, T3, T4, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}
class MultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> : MultiUpdateBase, IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5>
{
    #region Constructor
    public MultiUpdateJoin(MultipleQuery multiQuery, IUpdateVisitor visitor)
        : base(multiQuery, visitor) { }
    #endregion

    #region Set/SetRaw/SetWith/SetFrom
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.Set(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.Set(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
        => this.Set(true, fieldSelector, fieldValue);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (fieldValue == null)
            throw new ArgumentNullException(nameof(fieldValue));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.Set(fieldSelector, fieldValue);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetRaw(string rawSql, object updateObj)
       => this.SetRaw(true, rawSql, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetRaw(bool condition, string rawSql, object updateObj)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        if (condition) this.visitor.SetRaw(rawSql, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TupdateObj>(TupdateObj updateObj)
        => this.SetWith(true, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TupdateObj>(bool condition, TupdateObj updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        if (condition) this.visitor.SetWith(null, updateObj);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
        => this.SetWith(true, fieldsSelectorOrAssignment, updateObj);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetWith<TFields>(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, TFields>> fieldsSelectorOrAssignment, object updateObj)
    {
        if (fieldsSelectorOrAssignment == null)
            throw new ArgumentNullException(nameof(fieldsSelectorOrAssignment));
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        if (fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberAccess && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.New && fieldsSelectorOrAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelectorOrAssignment)},只支持MemberAccess、New、MemberInit三种类型表达式");

        if (condition) this.visitor.SetWith(fieldsSelectorOrAssignment, updateObj);
        return this;
    }

    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
        => this.SetFrom(true, fieldsAssignment);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TFields>(bool condition, Expression<Func<IFromQuery, TEntity, T1, T2, T3, T4, T5, TFields>> fieldsAssignment)
    {
        if (fieldsAssignment == null)
            throw new ArgumentNullException(nameof(fieldsAssignment));
        if (fieldsAssignment.Body.NodeType != ExpressionType.New && fieldsAssignment.Body.NodeType != ExpressionType.MemberInit)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsAssignment)},只支持New或MemberInit类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldsAssignment);
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
        => this.SetFrom(true, fieldSelector, valueSelector);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> SetFrom<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<IFromQuery, TEntity, IFromQuery<TField>>> valueSelector)
    {
        if (fieldSelector == null)
            throw new ArgumentNullException(nameof(fieldSelector));
        if (valueSelector == null)
            throw new ArgumentNullException(nameof(valueSelector));
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式");

        if (condition)
            this.visitor.SetFrom(fieldSelector, valueSelector);
        return this;
    }
    #endregion

    #region Where/And
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.Where(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.Where(ifPredicate);
        else if (elsePredicate != null) this.visitor.Where(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> predicate)
        => this.And(true, predicate);
    public IMultiUpdateJoin<TEntity, T1, T2, T3, T4, T5> And(bool condition, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> elsePredicate = null)
    {
        if (ifPredicate == null)
            throw new ArgumentNullException(nameof(ifPredicate));

        if (condition)
            this.visitor.And(ifPredicate);
        else if (elsePredicate != null) this.visitor.And(elsePredicate);
        this.hasWhere = true;
        return this;
    }
    #endregion
}