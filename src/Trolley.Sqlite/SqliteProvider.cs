using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.Sqlite;

public partial class SqliteProvider : BaseOrmProvider
{
    private readonly static Dictionary<object, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.Sqlite;
    public override Type NativeDbTypeType => typeof(DbType);
    public override string DefaultTableSchema => "main";

    static SqliteProvider()
    {
        defaultMapTypes[DbType.Boolean] = typeof(bool);
        defaultMapTypes[DbType.Byte] = typeof(byte);
        defaultMapTypes[DbType.SByte] = typeof(sbyte);
        defaultMapTypes[DbType.Int16] = typeof(short);
        defaultMapTypes[DbType.UInt16] = typeof(ushort);
        defaultMapTypes[DbType.Int32] = typeof(int);
        defaultMapTypes[DbType.UInt32] = typeof(uint);
        defaultMapTypes[DbType.Int64] = typeof(long);
        defaultMapTypes[DbType.UInt64] = typeof(ulong);
        defaultMapTypes[DbType.Single] = typeof(float);
        defaultMapTypes[DbType.Double] = typeof(double);
        defaultMapTypes[DbType.Currency] = typeof(decimal);
        defaultMapTypes[DbType.Decimal] = typeof(decimal);
        defaultMapTypes[DbType.AnsiStringFixedLength] = typeof(string);
        defaultMapTypes[DbType.AnsiString] = typeof(string);
        defaultMapTypes[DbType.String] = typeof(string);
        defaultMapTypes[DbType.DateTime] = typeof(DateTime);
        defaultMapTypes[DbType.DateTime2] = typeof(DateTime);
        defaultMapTypes[DbType.DateTimeOffset] = typeof(DateTimeOffset);
#if NET6_0_OR_GREATER
        defaultMapTypes[DbType.Date] = typeof(DateOnly);
        defaultMapTypes[DbType.Time] = typeof(TimeOnly);
#else
        defaultMapTypes[DbType.Date] = typeof(DateTime);
        defaultMapTypes[DbType.Time] = typeof(TimeSpan);
#endif
        defaultMapTypes[DbType.Guid] = typeof(Guid);
        defaultMapTypes[DbType.Binary] = typeof(byte[]); 
        defaultMapTypes[DbType.VarNumeric] = typeof(decimal);
        defaultMapTypes[DbType.Xml] = typeof(string);

        defaultDbTypes[typeof(bool)] = DbType.Boolean;
        defaultDbTypes[typeof(byte)] = DbType.Byte;
        defaultDbTypes[typeof(sbyte)] = DbType.SByte;
        defaultDbTypes[typeof(short)] = DbType.Int16;
        defaultDbTypes[typeof(ushort)] = DbType.UInt16;
        defaultDbTypes[typeof(int)] = DbType.Int32;
        defaultDbTypes[typeof(uint)] = DbType.UInt32;
        defaultDbTypes[typeof(long)] = DbType.Int64;
        defaultDbTypes[typeof(ulong)] = DbType.UInt64;
        defaultDbTypes[typeof(float)] = DbType.Single;
        defaultDbTypes[typeof(double)] = DbType.Double;
        defaultDbTypes[typeof(decimal)] = DbType.Decimal;
        defaultDbTypes[typeof(string)] = DbType.String;
        defaultDbTypes[typeof(DateTime)] = DbType.DateTime;
        defaultDbTypes[typeof(TimeSpan)] = DbType.Time;
        defaultDbTypes[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
        
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly)] = DbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = DbType.Time;
#endif
        defaultDbTypes[typeof(byte[])] = DbType.Binary;
        defaultDbTypes[typeof(Guid)] = DbType.String;

        defaultDbTypes[typeof(bool?)] = DbType.Int32;
        defaultDbTypes[typeof(byte?)] = DbType.Int32;
        defaultDbTypes[typeof(sbyte?)] = DbType.Int32;
        defaultDbTypes[typeof(short?)] = DbType.Int32;
        defaultDbTypes[typeof(ushort?)] = DbType.Int32;
        defaultDbTypes[typeof(int?)] = DbType.Int32;
        defaultDbTypes[typeof(uint?)] = DbType.Int64;
        defaultDbTypes[typeof(long?)] = DbType.Int64;
        defaultDbTypes[typeof(ulong?)] = DbType.Decimal;
        defaultDbTypes[typeof(float?)] = DbType.Single;
        defaultDbTypes[typeof(double?)] = DbType.Double;
        defaultDbTypes[typeof(decimal?)] = DbType.Decimal;
        defaultDbTypes[typeof(DateTime?)] = DbType.DateTime;
        defaultDbTypes[typeof(TimeSpan?)] = DbType.Time;
        defaultDbTypes[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly?)] = DbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = DbType.Time;
#endif
        defaultDbTypes[typeof(Guid?)] = DbType.String;


        castTos[typeof(string)] = "CHARACTER";
        castTos[typeof(byte)] = "TINYINT";
        castTos[typeof(sbyte)] = "TINYINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "INTEGER";
        castTos[typeof(int)] = "INTEGER";
        castTos[typeof(uint)] = "BIGINT";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "DECIMAL(36,0)";
        castTos[typeof(float)] = "FLOAT";
        castTos[typeof(double)] = "DOUBLE";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(bool)] = "BOOLEAN";
        castTos[typeof(DateTime)] = "DATETIME";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "BIGINT";
#endif
        castTos[typeof(Guid)] = "CHARACTER";

        castTos[typeof(string)] = "CHARACTER";
        castTos[typeof(byte?)] = "TINYINT";
        castTos[typeof(sbyte?)] = "TINYINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "INTEGER";
        castTos[typeof(int?)] = "INTEGER";
        castTos[typeof(uint?)] = "BIGINT";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "DECIMAL(36,0)";
        castTos[typeof(float?)] = "FLOAT";
        castTos[typeof(double?)] = "DOUBLE";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(bool?)] = "INTEGER";
        castTos[typeof(DateTime?)] = "TEXT";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "BIGINT";
#endif
        castTos[typeof(Guid?)] = "CHARACTER";
    }
    public override ITheaConnection CreateConnection(string dbKey, string connectionString)
        => new SqliteTheaConnection(dbKey, connectionString);
    public override IDbCommand CreateCommand() => new SQLiteCommand();
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new SQLiteParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => new SQLiteParameter(parameterName, (DbType)nativeDbType) { Value = value };
    public override void ChangeParameter(object dbParameter, Type targetType, object value)
    {
        var fieldValue = Convert.ChangeType(value, targetType);
        var myDbParameter = dbParameter as SQLiteParameter;
        var nativeDbType = (DbType)this.GetNativeDbType(targetType);
        myDbParameter.DbType = nativeDbType;
        myDbParameter.Value = fieldValue;
    }
    public override string GetTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        if (tableName.Contains('.'))
        {
            var tableNames = tableName.Split('.');
            if (tableNames[0] == this.DefaultTableSchema)
                return "\"" + tableNames[1] + "\"";
            return $"\"{tableNames[0]}\".\"{tableNames[1]}\"";
        }
        return "\"" + tableName + "\"";
    }
    public override string GetFieldName(string fieldName) => "\"" + fieldName + "\"";
    public override string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue && skip.Value > 0) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的System.Data.DbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override Type MapDefaultType(MemberMap memberMappper)
        => this.MapDefaultType(memberMappper.NativeDbType);
    public override string GetIdentitySql(string keyField) => ";SELECT LAST_INSERT_ROWID()";
    public override string CastTo(Type type, object value, string characterSetOrCollation = null)
        => $"CAST({value} AS {castTos[type]})";
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        switch (expectType)
        {
            case Type factType when factType == typeof(bool):
                return Convert.ToBoolean(value) ? "1" : "0";
            case Type factType when factType == typeof(string):
                return $"'{Convert.ToString(value).Replace("'", @"\'")}'";
            case Type factType when factType == typeof(Guid):
                return $"'{(Guid)value}'";
            case Type factType when factType == typeof(DateTime):
                return $"'{Convert.ToDateTime(value):yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fff}'";
            case Type factType when factType == typeof(DateTimeOffset):
                return $"'{(DateTimeOffset)value:yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fffZ}'";
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(DateOnly):
                return $"'{(DateOnly)value:yyyy\\-MM\\-dd}'";
#endif
            case Type factType when factType == typeof(TimeSpan):
                {
                    var factValue = (TimeSpan)value;
                    if (factValue.TotalDays > 1 || factValue.TotalDays < -1)
                        return $"'{(int)factValue.TotalDays}.{factValue:hh\\:mm\\:ss\\.ffffff}'";
                    return $"'{factValue:hh\\:mm\\:ss\\.ffffff}'";
                }
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(TimeOnly): return $"'{(TimeOnly)value:hh\\:mm\\:ss\\.ffffff}'";
#endif          
            case Type factType when factType == typeof(SqlFieldSegment):
                {
                    var sqlSegment = value as SqlFieldSegment;
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                        return this.GetQuotedValue(sqlSegment.Value);
                    return sqlSegment.Body;
                }
            default: return value.ToString();
        }
    }
    public override object MapNativeDbType(DbColumnInfo columnInfo)
    {
        switch (columnInfo.DataType)
        {
            case "boolean": return DbType.Boolean;

            case "int2":
            case "tinyint":
            case "smallint":
            case "mediumint":
            case "integer": return DbType.Int32;

            case "int8":
            case "bigint": return DbType.Int64;

            case "unsigned big int": return DbType.Decimal;

            case "float": return DbType.Single;
            case "double precision":
            case "double": return DbType.Double;
            case "real": return DbType.Decimal;

            case "date":
            case "datetime": return DbType.String;

            case "blob": return DbType.Binary;

            case "character(20)":
            case "varchar(255)":
            case "varying character(255)":
            case "nchar(55)":
            case "native character(70)":
            case "nvarchar(100)":

            case "varchar":
            case "varying character":
            case "nchar":
            case "nvarchar":
            case "text":
            case "clob": return DbType.String;

            default: return DbType.Object;
        }
    }
    public override void MapTables(string connectionString, IEntityMapProvider mapProvider)
    {
        var tableNames = mapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return;
        var builder = new StringBuilder();
        foreach (var tableName in tableNames)
        {
            builder.Append($"PRAGMA table_info({tableName});");
        }
        builder.Append("select name from sqlite_sequence");
        var sql = builder.ToString();
        var entityMappers = mapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        using var connection = new SQLiteConnection(connectionString);
        using var command = new SQLiteCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        DbTableInfo tableInfo = null;
        foreach (var tableName in tableNames)
        {
            tableInfo = new DbTableInfo
            {
                TableName = tableName,
                Columns = new List<DbColumnInfo>()
            };
            tableInfos.Add(tableInfo);
            while (reader.Read())
            {
                tableInfo.Columns.Add(new DbColumnInfo
                {
                    Position = reader.ToFieldValue<int>(0),
                    FieldName = reader.ToFieldValue<string>(1),
                    DbColumnType = reader.ToFieldValue<string>(2),
                    IsNullable = !reader.ToFieldValue<bool>(3),
                    DefaultValue = reader.ToFieldValue<string>(4),
                    IsPrimaryKey = reader.ToFieldValue<bool>(5)
                });
            }
            reader.NextResult();
        }
        var identityTables = new List<string>();
        if (reader.Read())
        {
            identityTables.Add(reader.ToFieldValue<string>(0));
        }
        reader.Close();
        connection.Close();
        var fieldMapHandler = mapProvider.FieldMapHandler;
        foreach (var entityMapper in entityMappers)
        {
            (var tableSchema, var tableName) = this.GetFullTableName(entityMapper.TableName);
            tableInfo = tableInfos.Find(f => f.TableSchema == tableSchema && f.TableName == tableName);
            if (tableInfo == null)
                continue;

            var memberInfos = entityMapper.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            var mappedMappers = new List<MemberMap>();
            bool isAutoIncrement = identityTables.Contains(tableName);
            foreach (var columnInfo in tableInfo.Columns)
            {
                var intTypes = new string[] { "int", "int2", "tinyint", "smallint", "mediumint", "integer", "int8", "bigint", "unsigned big int" };
                if (columnInfo.IsPrimaryKey && intTypes.Contains(columnInfo.DbColumnType.ToLower()))
                    columnInfo.IsAutoIncrement = identityTables.Contains(tableName);
                var index = columnInfo.DbColumnType.IndexOf('(');
                if (index >= 0)
                {
                    var endIndex = columnInfo.DbColumnType.IndexOf(')', index + 1);
                    columnInfo.MaxLength = int.Parse(columnInfo.DbColumnType.Substring(index + 1, endIndex - index));
                }
                columnInfo.DataType = columnInfo.DbColumnType.Substring(0, index).ToLower();

                if (fieldMapHandler.TryFindMember(columnInfo.FieldName, entityMapper.MemberMaps, out var memberMapper))
                {
                    memberMapper.DbColumnType = columnInfo.DbColumnType;
                    memberMapper.IsKey = columnInfo.IsPrimaryKey;
                    memberMapper.IsAutoIncrement = columnInfo.IsAutoIncrement;
                    memberMapper.IsRequired = !columnInfo.IsNullable;
                    memberMapper.MaxLength = columnInfo.MaxLength;
                    memberMapper.NativeDbType = this.MapNativeDbType(columnInfo);
                    memberMapper.Position = columnInfo.Position;
                }
                else
                {
                    if (!fieldMapHandler.TryFindMember(columnInfo.FieldName, memberInfos, out var memberInfo))
                    {
                        if (columnInfo.IsNullable)
                            continue;
                        throw new Exception($"表{tableName}非空字段{columnInfo.FieldName}在实体{entityMapper.EntityType.FullName}中没有对应映射成员或是不满足默认字段映射处理器DefaultFieldMapHandler规则，可手动配置映射字段如：.Member(f => f.XxxMember).Field(\"xxxField\")");
                    }

                    entityMapper.AddMemberMap(memberInfo.Name, memberMapper = new MemberMap(entityMapper, memberInfo)
                    {
                        FieldName = columnInfo.FieldName,
                        DbColumnType = columnInfo.DbColumnType,
                        IsKey = columnInfo.IsPrimaryKey,
                        IsAutoIncrement = columnInfo.IsAutoIncrement,
                        IsRequired = !columnInfo.IsNullable,
                        MaxLength = columnInfo.MaxLength,
                        NativeDbType = this.MapNativeDbType(columnInfo),
                        Position = columnInfo.Position
                    });
                }
                if (memberMapper.TypeHandler == null && !memberMapper.IsIgnore)
                {
                    //允许自定义TypeHandlerType设置，默认设置
                    if ((memberMapper.UnderlyingType.IsClass && memberMapper.UnderlyingType != typeof(string)
                        || memberMapper.UnderlyingType.IsEntityType(out _))
                        && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string))
                        memberMapper.TypeHandlerType = typeof(JsonTypeHandler);

                    //object类型
                    if (memberMapper.MemberType == typeof(object) && this.MapDefaultType(memberMapper) == typeof(string))
                        memberMapper.TypeHandlerType = typeof(ToStringTypeHandler);

                    if (memberMapper.TypeHandlerType != null)
                        memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
                if (memberMapper.DbColumnType.ToLower() == "timestamp")
                    memberMapper.IsRowVersion = true;
                mappedMappers.Add(memberMapper);
            }
            var ignoreMappers = entityMapper.MemberMaps.Except(mappedMappers).ToList();
            if (ignoreMappers.Count > 0)
            {
                foreach (var memberMapper in ignoreMappers)
                {
                    if (memberMapper.IsNavigation || memberMapper.IsRowVersion)
                        continue;
                    memberMapper.IsIgnore = true;
                }
            }
            //if (tableSchema != this.DefaultTableSchema)
            //    entityMapper.TableSchema = tableSchema;
            //else entityMapper.TableName = tableName;
            entityMapper.IsMapped = true;
        }
    }
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        int cacheKey = 0;
        switch (methodInfo.Name)
        {
            case "IsNull":
                cacheKey = RepositoryHelper.GetCacheKey(typeof(Sql), methodInfo.GetGenericMethodDefinition());
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                    var targetArgument = visitor.GetQuotedValue(targetSegment);
                    var rightArgument = visitor.GetQuotedValue(rightSegment);
                    return targetSegment.Merge(rightSegment, $"IFNULL({targetArgument},{rightArgument})", false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
    //public int ExecuteBulkCopy(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    //{
    //    var entityMapper = visitor.Tables[0].Mapper;
    //    var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
    //    var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
    //    if (dataTable.Rows.Count == 0) return 0;

    //    connection.Open();
    //    var dbConnection = connection.BaseConnection as SQLiteConnection;
    //    var transaction = dbContext.Transaction?.BaseTransaction as SQLiteTransaction;

    //    var bulkCopy = new SQLiteBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
    //    if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
    //    bulkCopy.DestinationTableName = dataTable.TableName;
    //    for (int i = 0; i < dataTable.Columns.Count; i++)
    //    {
    //        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
    //    }

    //    var createdAt = DateTime.Now;
    //    dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
    //    {
    //        DbKey = dbContext.DbKey,
    //        ConnectionString = connection.ConnectionString,
    //        SqlType = CommandSqlType.BulkCopyInsert
    //    });
    //    int recordsAffected = 0;
    //    bool isSuccess = true;
    //    Exception exception = null;
    //    try
    //    {
    //        bulkCopy.WriteToServer(dataTable);
    //        recordsAffected = dataTable.Rows.Count;
    //    }
    //    catch (Exception ex)
    //    {
    //        exception = ex;
    //        isSuccess = false;
    //    }
    //    finally
    //    {
    //        var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
    //        dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
    //        {
    //            DbKey = dbContext.DbKey,
    //            ConnectionString = connection.ConnectionString,
    //            SqlType = CommandSqlType.BulkCopyInsert,
    //            IsSuccess = isSuccess,
    //            Exception = exception,
    //            Elapsed = (int)elapsed
    //        });
    //    }
    //    if (!isSuccess)
    //    {
    //        if (transaction == null) connection.Close();
    //        throw exception;
    //    }
    //    return recordsAffected;
    //}
    //public async Task<int> ExecuteBulkCopyAsync(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    //{
    //    var entityMapper = visitor.Tables[0].Mapper;
    //    var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
    //    var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
    //    if (dataTable.Rows.Count == 0) return 0;

    //    await connection.OpenAsync(cancellationToken);
    //    var dbConnection = connection.BaseConnection as SqlConnection;
    //    var transaction = dbContext.Transaction?.BaseTransaction as SqlTransaction;
    //    var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
    //    if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
    //    bulkCopy.DestinationTableName = dataTable.TableName;
    //    for (int i = 0; i < dataTable.Columns.Count; i++)
    //    {
    //        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
    //    }

    //    var createdAt = DateTime.Now;
    //    dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
    //    {
    //        DbKey = dbContext.DbKey,
    //        ConnectionString = connection.ConnectionString,
    //        SqlType = CommandSqlType.BulkCopyInsert
    //    });
    //    int recordsAffected = 0;
    //    bool isSuccess = true;
    //    Exception exception = null;
    //    try
    //    {
    //        await bulkCopy.WriteToServerAsync(dataTable);
    //        recordsAffected = dataTable.Rows.Count;
    //    }
    //    catch (Exception ex)
    //    {
    //        exception = ex;
    //        isSuccess = false;
    //    }
    //    finally
    //    {
    //        var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
    //        dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
    //        {
    //            DbKey = dbContext.DbKey,
    //            ConnectionString = connection.ConnectionString,
    //            SqlType = CommandSqlType.BulkCopyInsert,
    //            IsSuccess = isSuccess,
    //            Exception = exception,
    //            Elapsed = (int)elapsed
    //        });
    //    }
    //    if (!isSuccess)
    //    {
    //        if (transaction == null) await connection.CloseAsync();
    //        throw exception;
    //    }
    //    return recordsAffected;
    //}
}