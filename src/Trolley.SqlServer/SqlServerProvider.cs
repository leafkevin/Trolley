using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

public partial class SqlServerProvider : BaseOrmProvider
{
    private readonly static Dictionary<object, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.SqlServer;
    public override Type NativeDbTypeType => typeof(SqlDbType);
    public override string DefaultTableSchema => "dbo";

    static SqlServerProvider()
    {
        defaultMapTypes[SqlDbType.Bit] = typeof(bool);
        defaultMapTypes[SqlDbType.TinyInt] = typeof(byte);
        defaultMapTypes[SqlDbType.SmallInt] = typeof(short);
        defaultMapTypes[SqlDbType.Int] = typeof(int);
        defaultMapTypes[SqlDbType.BigInt] = typeof(long);
        defaultMapTypes[SqlDbType.Real] = typeof(float);
        defaultMapTypes[SqlDbType.Float] = typeof(double);
        defaultMapTypes[SqlDbType.Decimal] = typeof(decimal);
        defaultMapTypes[SqlDbType.Money] = typeof(decimal);
        defaultMapTypes[SqlDbType.SmallMoney] = typeof(decimal);
        defaultMapTypes[SqlDbType.Char] = typeof(string);
        defaultMapTypes[SqlDbType.NChar] = typeof(string);
        defaultMapTypes[SqlDbType.VarChar] = typeof(string);
        defaultMapTypes[SqlDbType.NVarChar] = typeof(string);
        defaultMapTypes[SqlDbType.Text] = typeof(string);
        defaultMapTypes[SqlDbType.NText] = typeof(string);
        defaultMapTypes[SqlDbType.SmallDateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.Timestamp] = typeof(byte[]);
        defaultMapTypes[SqlDbType.DateTime2] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTimeOffset] = typeof(DateTimeOffset);
#if NET6_0_OR_GREATER
        defaultMapTypes[SqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[SqlDbType.Time] = typeof(TimeOnly);
#else
        defaultMapTypes[SqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[SqlDbType.Time] = typeof(TimeSpan);
#endif
        defaultMapTypes[SqlDbType.Image] = typeof(byte[]);
        defaultMapTypes[SqlDbType.Binary] = typeof(byte[]);
        defaultMapTypes[SqlDbType.VarBinary] = typeof(byte[]);
        defaultMapTypes[SqlDbType.UniqueIdentifier] = typeof(Guid);

        defaultDbTypes[typeof(bool)] = SqlDbType.Bit;
        defaultDbTypes[typeof(byte)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(sbyte)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(ushort)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int)] = SqlDbType.Int;
        defaultDbTypes[typeof(uint)] = SqlDbType.Int;
        defaultDbTypes[typeof(long)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(ulong)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(float)] = SqlDbType.Real;
        defaultDbTypes[typeof(double)] = SqlDbType.Float;
        defaultDbTypes[typeof(decimal)] = SqlDbType.Decimal;
        defaultDbTypes[typeof(string)] = SqlDbType.NVarChar;
        defaultDbTypes[typeof(DateTime)] = SqlDbType.DateTime;
        defaultDbTypes[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly)] = SqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = SqlDbType.Time;
#endif
        defaultDbTypes[typeof(byte[])] = SqlDbType.VarBinary;
        defaultDbTypes[typeof(Guid)] = SqlDbType.UniqueIdentifier;

        defaultDbTypes[typeof(bool?)] = SqlDbType.Bit;
        defaultDbTypes[typeof(byte?)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(sbyte?)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short?)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(ushort?)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int?)] = SqlDbType.Int;
        defaultDbTypes[typeof(uint?)] = SqlDbType.Int;
        defaultDbTypes[typeof(long?)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(ulong?)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(float?)] = SqlDbType.Real;
        defaultDbTypes[typeof(double?)] = SqlDbType.Float;
        defaultDbTypes[typeof(decimal?)] = SqlDbType.Decimal;
        defaultDbTypes[typeof(DateTime?)] = SqlDbType.DateTime;
        defaultDbTypes[typeof(DateTimeOffset?)] = SqlDbType.DateTimeOffset;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly?)] = SqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = SqlDbType.Time;
#endif
        defaultDbTypes[typeof(Guid?)] = SqlDbType.UniqueIdentifier;


        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte)] = "TINYINT";
        castTos[typeof(sbyte)] = "TINYINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "SMALLINT";
        castTos[typeof(int)] = "INT";
        castTos[typeof(uint)] = "INT";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "BIGINT";
        castTos[typeof(float)] = "REAL";
        castTos[typeof(double)] = "FLOAT";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(bool)] = "BIT";
        castTos[typeof(DateTime)] = "DATETIME";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";
#endif
        castTos[typeof(Guid)] = "UNIQUEIDENTIFIER";

        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte?)] = "TINYINT";
        castTos[typeof(sbyte?)] = "TINYINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "SMALLINT";
        castTos[typeof(int?)] = "INT";
        castTos[typeof(uint?)] = "INT";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "BIGINT";
        castTos[typeof(float?)] = "REAL";
        castTos[typeof(double?)] = "FLOAT";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(bool?)] = "BIT";
        castTos[typeof(DateTime?)] = "DATETIME";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
#endif
        castTos[typeof(Guid?)] = "UNIQUEIDENTIFIER";
    }
    public override ITheaConnection CreateConnection(string dbKey, string connectionString)
        => new SqlServerTheaConnection(dbKey, connectionString);
    public override IDbCommand CreateCommand() => new SqlCommand();
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new SqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => new SqlParameter(parameterName, (SqlDbType)nativeDbType) { Value = value };
    public override void ChangeParameter(object dbParameter, Type targetType, object value)
    {
        var fieldValue = Convert.ChangeType(value, targetType);
        var myDbParameter = dbParameter as SqlParameter;
        var nativeDbType = (SqlDbType)this.GetNativeDbType(targetType);
        myDbParameter.SqlDbType = nativeDbType;
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
                return "[" + tableNames[1] + "]";
            return $"[{tableNames[0]}].[{tableNames[1]}]";
        }
        return "[" + tableName + "]";
    }
    public override string GetFieldName(string fieldName) => "[" + fieldName + "]";
    public override string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT ");
        if (skip.HasValue && skip.Value > 0 && limit.HasValue)
        {
            if (string.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
            builder.Append($" OFFSET {skip} ROWS");
            builder.AppendFormat($" FETCH NEXT {limit} ROWS ONLY", limit);
        }
        else if (limit.HasValue)
        {
            builder.Append($"TOP {limit} ");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        }
        return builder.ToString();
    }
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的System.Data.SqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string GetIdentitySql(string keyField) => ";SELECT SCOPE_IDENTITY()";
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
                return $"N'{Convert.ToString(value).Replace("'", @"\'")}'";
            case Type factType when factType == typeof(Guid):
                return $"'{value}'";
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
            case "bit": return SqlDbType.Bit;

            case "char": return SqlDbType.Char;
            case "varchar": return SqlDbType.VarChar;
            case "text": return SqlDbType.Text;
            case "nchar": return SqlDbType.NChar;
            case "nvarchar": return SqlDbType.NVarChar;
            case "ntext": return SqlDbType.NText;

            case "tinyint": return SqlDbType.TinyInt;
            case "smallint": return SqlDbType.SmallInt;
            case "int": return SqlDbType.Int;
            case "bigint": return SqlDbType.NText;

            case "smalldatetime": return SqlDbType.SmallDateTime;
            case "datetime": return SqlDbType.DateTime;
            case "datetime2": return SqlDbType.DateTime2;
            case "date": return SqlDbType.Date;
            case "time": return SqlDbType.Time;
            case "datetimeoffset": return SqlDbType.DateTimeOffset;

            case "real": return SqlDbType.Real;
            case "float": return SqlDbType.Float;
            case "numeric": return SqlDbType.Decimal;
            case "smallmoney": return SqlDbType.SmallMoney;
            case "decimal": return SqlDbType.Decimal;
            case "money": return SqlDbType.Money;
            case "image": return SqlDbType.Image;

            case "binary": return SqlDbType.Binary;
            case "varbinary": return SqlDbType.VarBinary;
            case "timestamp": return SqlDbType.Timestamp;
            case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
            default: return SqlDbType.Variant;
        }
    }
    public override void MapTables(string connectionString, IEntityMapProvider mapProvider)
    {
        var tableNames = mapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return;
        var sql = @"select b.name,a.name,c.name,d.name,(d.name+case when d.name in ('char','varchar','nchar','nvarchar','binary','varbinary') then '('+ case when c.max_length = -1 then 'MAX' when d.name in ('nchar','nvarchar') then
cast(c.max_length/2 as varchar) else cast(c.max_length as varchar) end+')' when d.name in ('numeric','decimal') then '('+cast(c.precision as varchar)+','+ cast(c.scale as varchar)+')' else '' end),case when d.name in ('nchar','nvarchar')
then c.max_length/2 else c.max_length end,c.scale,c.precision,(select value from sys.extended_properties where major_id=c.object_id AND minor_id=c.column_id AND name = 'MS_Description'and class=1),e.text,g.is_primary_key,c.is_identity,
c.is_nullable,c.column_id from sys.tables a inner join sys.schemas b on a.schema_id=b.schema_id inner join sys.columns c on a.object_id=c.object_id inner join sys.types d on d.user_type_id=c.user_type_id left join syscomments e
on e.id = c.default_object_id left join sys.index_columns f on f.object_id=a.object_id and f.column_id=c.column_id left join sys.indexes g on g.object_id=a.object_id and g.index_id=f.index_id WHERE {0} order by b.name,a.name,c.column_id";
        var tableBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableName in tableNames)
        {
            StringBuilder builder = null;
            string myTableName = null;
            if (tableName.Contains('.'))
            {
                var myTableNames = tableName.Split('.');
                var tableSchema = myTableNames[0];
                myTableName = myTableNames[1];
                if (!tableBuilders.TryGetValue(tableSchema, out builder))
                    tableBuilders.Add(tableSchema, builder = new StringBuilder());
            }
            else
            {
                var tableSchema = this.DefaultTableSchema;
                if (!tableBuilders.TryGetValue(tableSchema, out builder))
                    tableBuilders.Add(tableSchema, builder = new StringBuilder());
                myTableName = tableName;
            }
            if (builder.Length > 0)
                builder.Append(',');
            builder.Append($"'{myTableName}'");
        }
        var sqlBuilder = new StringBuilder();
        foreach (var tableBuilder in tableBuilders)
        {
            if (sqlBuilder.Length > 0)
                sqlBuilder.Append(" OR ");

            sqlBuilder.Append($"b.name='{tableBuilder.Key}' AND a.name IN ({tableBuilder.Value.ToString()})");
        }
        sql = string.Format(sql, sqlBuilder.ToString());
        var entityMappers = mapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        DbTableInfo tableInfo = null;
        while (reader.Read())
        {
            var tableSchema = reader.ToFieldValue<string>(0);
            var tableName = reader.ToFieldValue<string>(1);
            if (tableInfo == null || tableInfo.TableSchema != tableSchema || tableInfo.TableName != tableName)
            {
                tableInfo = new DbTableInfo
                {
                    TableSchema = tableSchema,
                    TableName = tableName,
                    Columns = new List<DbColumnInfo>()
                };
                tableInfos.Add(tableInfo);
            }
            tableInfo.Columns.Add(new DbColumnInfo
            {
                FieldName = reader.ToFieldValue<string>(2),
                DataType = reader.ToFieldValue<string>(3),
                DbColumnType = reader.ToFieldValue<string>(4),
                MaxLength = (int)reader.ToFieldValue<ulong>(5),
                Scale = reader.ToFieldValue<int>(6),
                Precision = reader.ToFieldValue<int>(7),
                Description = reader.ToFieldValue<string>(8),
                DefaultValue = reader.ToFieldValue<string>(9),
                IsPrimaryKey = reader.ToFieldValue<bool>(10),
                IsAutoIncrement = reader.ToFieldValue<bool>(11),
                IsNullable = reader.ToFieldValue<bool>(12),
                Position = reader.ToFieldValue<int>(13)
            });
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

            foreach (var columnInfo in tableInfo.Columns)
            {
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
                        if (columnInfo.IsNullable || memberMapper.DbColumnType.ToLower() == "timestamp")
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

                //允许自定义TypeHandlerType设置，默认设置
                if ((memberMapper.UnderlyingType.IsClass && memberMapper.UnderlyingType != typeof(string) || memberMapper.UnderlyingType.IsEntityType(out _))
                    && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string) && memberMapper.TypeHandlerType == null)
                    memberMapper.TypeHandlerType = typeof(JsonTypeHandler);

                if (memberMapper.TypeHandlerType != null && memberMapper.TypeHandler == null)
                    memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                //object类型
                if (memberMapper.MemberType == typeof(object) && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string))
                {
                    memberMapper.TypeHandlerType = typeof(ToStringTypeHandler);
                    memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
                if (memberMapper.DbColumnType.ToLower() == "timestamp")
                    memberMapper.IsRowVersion = true;
            }

            //非默认TableSchema表名就不变更了
            if (tableSchema != this.DefaultTableSchema)
                entityMapper.TableSchema = tableSchema;
            else entityMapper.TableName = tableName;
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
                    return targetSegment.Merge(rightSegment, $"ISNULL({targetArgument},{rightArgument})", false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
    public int ExecuteBulkCopy(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
        var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        connection.Open();
        var dbConnection = connection.BaseConnection as SqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
        bulkCopy.DestinationTableName = dataTable.TableName;
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }

        var createdAt = DateTime.Now;
        dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = dbContext.DbKey,
            ConnectionString = connection.ConnectionString,
            SqlType = CommandSqlType.BulkCopyInsert
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            bulkCopy.WriteToServer(dataTable);
            recordsAffected = dataTable.Rows.Count;
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = dbContext.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = CommandSqlType.BulkCopyInsert,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteBulkCopyAsync(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, int? timeoutSeconds, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
        var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        await connection.OpenAsync(cancellationToken);
        var dbConnection = connection.BaseConnection as SqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as SqlTransaction;
        var bulkCopy = new SqlBulkCopy(dbConnection, SqlBulkCopyOptions.Default, transaction);
        if (timeoutSeconds.HasValue) bulkCopy.BulkCopyTimeout = timeoutSeconds.Value;
        bulkCopy.DestinationTableName = dataTable.TableName;
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, dataTable.Columns[i].ColumnName));
        }

        var createdAt = DateTime.Now;
        dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = dbContext.DbKey,
            ConnectionString = connection.ConnectionString,
            SqlType = CommandSqlType.BulkCopyInsert
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            await bulkCopy.WriteToServerAsync(dataTable);
            recordsAffected = dataTable.Rows.Count;
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = dbContext.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = CommandSqlType.BulkCopyInsert,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
        }
        if (!isSuccess)
        {
            if (transaction == null) await connection.CloseAsync();
            throw exception;
        }
        return recordsAffected;
    }
}