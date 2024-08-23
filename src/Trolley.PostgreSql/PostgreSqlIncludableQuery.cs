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
    public override IPostgreSqlIncludableQuery<T, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T, TMember>;
    public override IPostgreSqlIncludableQuery<T, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T, TMember>;
    public override IPostgreSqlIncludableQuery<T, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T, TMember>;
    public override IPostgreSqlIncludableQuery<T, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T, TMember>;
    public override IPostgreSqlIncludableQuery<T, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T, TMember>;
    public override IPostgreSqlIncludableQuery<T, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T, TNavigation>;
    public override IPostgreSqlIncludableQuery<T, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, TMember> : IncludableQuery<T1, T2, TMember>, IPostgreSqlIncludableQuery<T1, T2, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, TMember> : IncludableQuery<T1, T2, T3, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> : IncludableQuery<T1, T2, T3, T4, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> : IncludableQuery<T1, T2, T3, T4, T5, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion

    #region Sharding
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(params string[] tableNames)
        => base.UseTable(tableNames) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable(Func<string, bool> tableNamePredicate)
        => base.UseTable(tableNamePredicate) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)
        => base.UseTable<TMasterSharding>(tableNameGetter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableBy(object field1Value, object field2Value = null)
        => base.UseTableBy(field1Value, field2Value) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object beginFieldValue, object endFieldValue)
        => base.UseTableByRange(beginFieldValue, endFieldValue) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3)
        => base.UseTableByRange(fieldValue1, fieldValue2, fieldValue3) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    #endregion

    #region UseTableSchema
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> UseTableSchema(string tableSchema)
        => base.UseTableSchema(tableSchema) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>;
    #endregion

    #region ThenInclude/ThenIncludeMany
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation> ThenInclude<TNavigation>(Expression<Func<TMember, TNavigation>> member)
        => base.ThenInclude(member) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TNavigation>;
    public override IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> ThenIncludeMany<TElment>(Expression<Func<TMember, IEnumerable<TElment>>> member, Expression<Func<TElment, bool>> filter = null)
        => base.ThenIncludeMany(member, filter) as IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment>;
    #endregion
}
public class PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> : IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>, IPostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>
{
    #region Constructor
    public PostgreSqlIncludableQuery(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany)
        : base(dbContext, visitor, isIncludeMany) { }
    #endregion
}