using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public class PostgreSqlIncludableQuery<T, TMember> : IncludableQuery<T, TMember>, IPostgreSqlIncludableQuery<T, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T, TMember>;
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T, TNavigation>;
    public new IPostgreSqlIncludableQuery<T, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, TMember> : IncludableQuery<T1, T2, TMember>, IPostgreSqlIncludableQuery<T1, T2, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, TMember> : IncludableQuery<T1, T2, T3, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> : IncludableQuery<T1, T2, T3, T4, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> : IncludableQuery<T1, T2, T3, T4, T5, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTableMap<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(params object[] fieldValues)
        => base.UseTableBy(fieldValues) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableBy(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object beginField2Value, object endField2Value)
        => base.UseTableByRange(field1Value, beginField2Value, endField2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value)
        => base.UseTableByRange(field1Value, field2Value, beginField3Value, endField3Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    #endregion

    #region UseTableSchema
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation>;
    public new IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> ThenIncludeMany<TElement>(Expression<Func<TMember, IEnumerable<TElement>>> member, Expression<Func<TElement, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion
}