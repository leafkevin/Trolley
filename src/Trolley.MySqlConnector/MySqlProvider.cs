using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public partial class MySqlProvider : BaseOrmProvider
{
    private readonly static Dictionary<MySqlDbType, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.MySql;
    public override Type NativeDbTypeType => typeof(MySqlDbType);

    static MySqlProvider()
    {
        defaultMapTypes[MySqlDbType.Bit] = typeof(ulong);
        defaultMapTypes[MySqlDbType.Bool] = typeof(bool);
        defaultMapTypes[MySqlDbType.Byte] = typeof(sbyte);
        defaultMapTypes[MySqlDbType.UByte] = typeof(byte);
        defaultMapTypes[MySqlDbType.Int16] = typeof(short);
        defaultMapTypes[MySqlDbType.UInt16] = typeof(ushort);
        defaultMapTypes[MySqlDbType.Int24] = typeof(int);
        defaultMapTypes[MySqlDbType.UInt24] = typeof(uint);
        defaultMapTypes[MySqlDbType.Int32] = typeof(int);
        defaultMapTypes[MySqlDbType.UInt32] = typeof(uint);
        defaultMapTypes[MySqlDbType.Int64] = typeof(long);
        defaultMapTypes[MySqlDbType.UInt64] = typeof(ulong);
        defaultMapTypes[MySqlDbType.Float] = typeof(float);
        defaultMapTypes[MySqlDbType.Double] = typeof(double);
        defaultMapTypes[MySqlDbType.NewDecimal] = typeof(decimal);
        defaultMapTypes[MySqlDbType.Decimal] = typeof(decimal);
        defaultMapTypes[MySqlDbType.String] = typeof(string);
        defaultMapTypes[MySqlDbType.VarString] = typeof(string);
        defaultMapTypes[MySqlDbType.VarChar] = typeof(string);
        defaultMapTypes[MySqlDbType.TinyText] = typeof(string);
        defaultMapTypes[MySqlDbType.MediumText] = typeof(string);
        defaultMapTypes[MySqlDbType.LongText] = typeof(string);
        defaultMapTypes[MySqlDbType.Text] = typeof(string);
        defaultMapTypes[MySqlDbType.JSON] = typeof(string);
        defaultMapTypes[MySqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Newdate] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Timestamp] = typeof(DateTime);
#if NET6_0_OR_GREATER
        defaultMapTypes[MySqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[MySqlDbType.Time] = typeof(TimeOnly);
#else
        defaultMapTypes[MySqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Time] = typeof(TimeSpan);
#endif
        defaultMapTypes[MySqlDbType.Year] = typeof(int);
        defaultMapTypes[MySqlDbType.TinyBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.MediumBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.LongBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Blob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Binary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.VarBinary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Guid] = typeof(Guid);
        defaultMapTypes[MySqlDbType.Enum] = typeof(string);
        defaultMapTypes[MySqlDbType.Set] = typeof(string);

        defaultDbTypes[typeof(bool)] = MySqlDbType.Bool;
        defaultDbTypes[typeof(sbyte)] = MySqlDbType.Byte;
        defaultDbTypes[typeof(byte)] = MySqlDbType.UByte;
        defaultDbTypes[typeof(short)] = MySqlDbType.Int16;
        defaultDbTypes[typeof(ushort)] = MySqlDbType.UInt16;
        defaultDbTypes[typeof(int)] = MySqlDbType.Int32;
        defaultDbTypes[typeof(uint)] = MySqlDbType.UInt32;
        defaultDbTypes[typeof(long)] = MySqlDbType.Int64;
        defaultDbTypes[typeof(ulong)] = MySqlDbType.UInt64;
        defaultDbTypes[typeof(float)] = MySqlDbType.Float;
        defaultDbTypes[typeof(double)] = MySqlDbType.Double;
        defaultDbTypes[typeof(decimal)] = MySqlDbType.Decimal;
        defaultDbTypes[typeof(string)] = MySqlDbType.VarChar;
        defaultDbTypes[typeof(DateTime)] = MySqlDbType.DateTime;
        defaultDbTypes[typeof(TimeSpan)] = MySqlDbType.Time;
        defaultDbTypes[typeof(DateTimeOffset)] = MySqlDbType.Timestamp;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = MySqlDbType.Time;
#endif
        defaultDbTypes[typeof(Guid)] = MySqlDbType.Guid;
        defaultDbTypes[typeof(byte[])] = MySqlDbType.VarBinary;

        defaultDbTypes[typeof(bool?)] = MySqlDbType.Bool;
        defaultDbTypes[typeof(sbyte?)] = MySqlDbType.Byte;
        defaultDbTypes[typeof(byte?)] = MySqlDbType.UByte;
        defaultDbTypes[typeof(short?)] = MySqlDbType.Int16;
        defaultDbTypes[typeof(ushort?)] = MySqlDbType.UInt16;
        defaultDbTypes[typeof(int?)] = MySqlDbType.Int32;
        defaultDbTypes[typeof(uint?)] = MySqlDbType.UInt32;
        defaultDbTypes[typeof(long?)] = MySqlDbType.Int64;
        defaultDbTypes[typeof(ulong?)] = MySqlDbType.UInt64;
        defaultDbTypes[typeof(float?)] = MySqlDbType.Float;
        defaultDbTypes[typeof(double?)] = MySqlDbType.Double;
        defaultDbTypes[typeof(decimal?)] = MySqlDbType.Decimal;
        defaultDbTypes[typeof(DateTime?)] = MySqlDbType.DateTime;
        defaultDbTypes[typeof(TimeSpan?)] = MySqlDbType.Time;
        defaultDbTypes[typeof(DateTimeOffset?)] = MySqlDbType.Timestamp;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly?)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = MySqlDbType.Time;
#endif
        defaultDbTypes[typeof(Guid?)] = MySqlDbType.Guid;

        castTos[typeof(string)] = "CHAR";
        castTos[typeof(bool)] = "SIGNED";
        castTos[typeof(byte)] = "UNSIGNED";
        castTos[typeof(sbyte)] = "SIGNED";
        castTos[typeof(short)] = "SIGNED";
        castTos[typeof(ushort)] = "UNSIGNED";
        castTos[typeof(int)] = "SIGNED";
        castTos[typeof(uint)] = "UNSIGNED";
        castTos[typeof(long)] = "SIGNED";
        castTos[typeof(ulong)] = "UNSIGNED";
        castTos[typeof(float)] = "FLOAT";
        castTos[typeof(double)] = "DOUBLE";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime)] = "DATETIME";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";
#endif
        castTos[typeof(bool?)] = "SIGNED";
        castTos[typeof(byte?)] = "UNSIGNED";
        castTos[typeof(sbyte?)] = "SIGNED";
        castTos[typeof(short?)] = "SIGNED";
        castTos[typeof(ushort?)] = "UNSIGNED";
        castTos[typeof(int?)] = "SIGNED";
        castTos[typeof(uint?)] = "UNSIGNED";
        castTos[typeof(long?)] = "SIGNED";
        castTos[typeof(ulong?)] = "UNSIGNED";
        castTos[typeof(float?)] = "FLOAT";
        castTos[typeof(double?)] = "DOUBLE";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime?)] = "DATETIME";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
#endif
    }
    public override string GetDefaultSchema(string connectionString)
        => this.GetSchemaName(connectionString);
    public override ITheaConnection CreateConnection(string dbKey, string connectionString)
        => new MySqlTheaConnection(dbKey, connectionString);
    //public override ITheaCommand CreateCommand() => new MySqlTheaCommand(new MySqlCommand());
    public override IDbDataParameter CreateParameter(string parameterName, object value)
       => new MySqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => new MySqlParameter(parameterName, (MySqlDbType)nativeDbType) { Value = value };
    public override void ChangeParameter(object dbParameter, Type targetType, object value)
    {
        var fieldValue = Convert.ChangeType(value, targetType);
        var myDbParameter = dbParameter as MySqlParameter;
        var nativeDbType = (MySqlDbType)this.GetNativeDbType(targetType);
        myDbParameter.MySqlDbType = nativeDbType;
        myDbParameter.Value = fieldValue;
    }
    public override string GetTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        if (tableName.Contains('.'))
        {
            var tableNames = tableName.Split('.');
            return $"`{tableNames[0]}`.`{tableNames[1]}`";
        }
        return "`" + tableName + "`";
    }
    public override string GetFieldName(string fieldName) => "`" + fieldName + "`";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的MySqlConnector.MySqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(MemberMap memberMappper)
    {

        //bit(n)，会映射为ulong类型，bit(1)映射为bool类型
        if (memberMappper.NativeDbType is MySqlDbType nativeDbType)
        {
            if (nativeDbType == MySqlDbType.Bit)
            {
                if (memberMappper.MaxLength > 1)
                    return typeof(ulong);
                else return typeof(bool);
            }
            if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
                return result;
        }
        return typeof(object);
    }
    public override string CastTo(Type type, object value, string characterSetOrCollation = null)
    {
        if (string.IsNullOrEmpty(characterSetOrCollation))
            return $"CAST({value} AS {castTos[type]})";
        return $"CAST({value} AS {castTos[type]} {characterSetOrCollation})";
    }
    public override object MapNativeDbType(DbColumnInfo columnInfo)
    {
        bool isUnsigned = columnInfo.DbColumnType.Contains("unsigned");
        switch (columnInfo.DataType)
        {
            case "bit": return MySqlDbType.Bit;
            case "bool": return MySqlDbType.Bool;
            case "tinyint":
                if (columnInfo.DbColumnType == "tinyint(1)")
                    return MySqlDbType.Bool;
                else return isUnsigned ? MySqlDbType.UByte : MySqlDbType.Byte;
            case "smallint": return isUnsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16;
            case "mediumint": return isUnsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24;
            case "int": return isUnsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32;
            case "bigint": return isUnsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64;
            case "float": return MySqlDbType.Float;
            case "real":
            case "double": return MySqlDbType.Double;
            case "numeric":
            case "decimal": return MySqlDbType.Decimal;
            case "year": return MySqlDbType.Year;
            case "time": return MySqlDbType.Time;
            case "date": return MySqlDbType.Date;
            case "timestamp": return MySqlDbType.Timestamp;
            case "smalldatetime":
            case "datetime": return MySqlDbType.DateTime;
            case "tinyblob": return MySqlDbType.TinyBlob;
            case "blob": return MySqlDbType.Blob;
            case "mediumblob": return MySqlDbType.MediumBlob;
            case "longblob": return MySqlDbType.LongBlob;
            case "binary": return MySqlDbType.Binary;
            case "varbinary": return MySqlDbType.VarBinary;
            case "tinytext": return MySqlDbType.TinyText;
            case "text": return MySqlDbType.Text;
            case "mediumtext": return MySqlDbType.MediumText;
            case "longtext": return MySqlDbType.LongText;
            case "char": return columnInfo.MaxLength == 36 ? MySqlDbType.Guid : MySqlDbType.String;
            case "varchar": return MySqlDbType.VarChar;
            case "set": return MySqlDbType.Set;
            case "enum": return MySqlDbType.Enum;
            case "point":
            case "linestring":
            case "polygon":
            case "geometry":
            case "multipoint":
            case "multilinestring":
            case "multipolygon":
            case "geometrycollection": return MySqlDbType.Geometry;
            default: return MySqlDbType.String;
        }
    }
    public override bool MapTables(string connectionString, IEntityMapProvider entityMapProvider, OrmDbFactoryOptions options)
    {
        var tableNames = entityMapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return true;
        var sql = @"SELECT a.TABLE_SCHEMA,a.TABLE_NAME,a.COLUMN_NAME,a.DATA_TYPE,a.COLUMN_TYPE,a.CHARACTER_MAXIMUM_LENGTH,a.NUMERIC_SCALE,a.NUMERIC_PRECISION,a.COLUMN_COMMENT,a.COLUMN_DEFAULT,
		a.COLUMN_KEY='PRI',INSTR(IFNULL(a.EXTRA,''),'auto_increment'),a.IS_NULLABLE='YES',a.ORDINAL_POSITION FROM INFORMATION_SCHEMA.COLUMNS a WHERE {0} ORDER BY a.TABLE_SCHEMA,a.TABLE_NAME,a.ORDINAL_POSITION";

        using var connection = new MySqlConnection(connectionString);
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
                var tableSchema = connection.Database;
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

            sqlBuilder.Append($"a.TABLE_SCHEMA='{tableBuilder.Key}' AND a.TABLE_NAME IN ({tableBuilder.Value.ToString()})");
        }
        sql = string.Format(sql, sqlBuilder.ToString());
        var entityMappers = entityMapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        using var command = new MySqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        DbTableInfo tableInfo = null;
        var lengthTypes = new[] { "bit", "binary", "varbinary" };
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
            var conlumnInfo = new DbColumnInfo
            {
                TableName = tableName,
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
            };
            tableInfo.Columns.Add(conlumnInfo);
            if (lengthTypes.Contains(conlumnInfo.DataType))
            {
                var beginIndex = conlumnInfo.DbColumnType.IndexOf('(') + 1;
                var endIndex = conlumnInfo.DbColumnType.IndexOf(')');
                var length = conlumnInfo.DbColumnType.Substring(beginIndex, endIndex - beginIndex);
                conlumnInfo.MaxLength = int.Parse(length);
            }
        }
        reader.Close();
        connection.Close();

        foreach (var entityMapper in entityMappers)
        {
            (var tableSchema, var tableName) = this.GetFullTableName(entityMapper.TableName);
            tableSchema ??= connection.Database;
            tableInfo = tableInfos.Find(f => f.TableSchema == tableSchema && f.TableName == tableName);
            if (tableInfo == null)
                continue;

            var memberInfos = entityMapper.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();

            var mappedMappers = new List<MemberMap>();
            foreach (var columnInfo in tableInfo.Columns)
            {
                if (entityMapProvider.TryMapMember(columnInfo.FieldName, entityMapper.MemberMaps, out var memberMapper))
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
                    if (!entityMapProvider.TryMapMember(columnInfo.FieldName, memberInfos, out var memberInfo))
                    {
                        if (columnInfo.IsNullable)
                            continue;
                        throw new Exception($"表{tableName}非空字段{columnInfo.FieldName}在实体{entityMapper.EntityType.FullName}中没有对应映射成员或是不满足默认字段映射处理器DefaultFieldMapHandler规则，可手动配置映射字段如：.Member(f => f.XxxMember).Field(\"xxxField\")，如果是RowVersion字段，需要手动指定，如：.Member(f => f.XxxMember).Field(\"xxxField\").RowVersion()");
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
                memberMapper.MappedTargetType = this.MapDefaultType(memberMapper);
                if (memberMapper.TypeHandler == null && !memberMapper.IsIgnore)
                {
                    if (options.IsAutoMapJsonTypeHandler && (memberMapper.UnderlyingType.IsClass && memberMapper.UnderlyingType != typeof(string)
                        || memberMapper.UnderlyingType.IsEntityType(out _))
                        && memberMapper.MappedTargetType == typeof(string))
                        memberMapper.TypeHandlerType = typeof(JsonTypeHandler);

                    if (memberMapper.TypeHandlerType != null)
                        memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
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

            //不更新TableSchema
            entityMapper.TableName = tableName;
            entityMapper.IsMapped = true;
        }
        return entityMapProvider.EntityMaps.Count(f => !f.IsMapped) == 0;
    }
    public virtual string GetSchemaName(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        return builder.Database;
    }
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        int cacheKey = 0;
        switch (methodInfo.Name)
        {
            case "Values":
                if (methodInfo.DeclaringType == typeof(MySqlExtensions))
                {
                    cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo.GetGenericMethodDefinition());
                    //.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var dialectVisitor = visitor as MySqlCreateVisitor;
                        if (methodCallExpr.Arguments[1] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Values方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Values(f.TotalAmount)}})");
                        if (!dialectVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{dialectVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        //使用别名，一定要先使用，后使用的话，存在表达式计算场景无法解析，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                        var fieldName = this.GetFieldName(memberMapper.FieldName);
                        //忽略更新别名
                        if (!dialectVisitor.IsUseSetAlias)
                            fieldName = $"VALUES({fieldName})";
                        return new SqlSegment
                        {
                            SqlType = SqlType.MethodCall,
                            MemberMapper = memberMapper,
                            TargetMember = memberMapper.Member,
                            MappedTargetType = memberMapper.MappedTargetType,
                            TypeHandler = memberMapper.TypeHandler,
                            Value = fieldName
                        };
                    });
                    return true;
                }
                break;
            case "IsNull":
                cacheKey = HashCode.Combine(typeof(Sql), methodInfo.GetGenericMethodDefinition());
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    var targetArgument = visitor.WrapSql(targetSegment);
                    var rightArgument = visitor.WrapSql(rightSegment);
                    return targetSegment.Change($"IFNULL({targetArgument},{rightArgument})", SqlType.MethodCall);
                });
                return true;
        }
        formatter = null;
        return false;
    }
    public int ExecuteBulkCopy(string tableName, MySqlBulkCopy bulkCopyObj, DbContext dbContext, ITheaConnection connection, IDataReader dataReader)
    {
        bulkCopyObj.DestinationTableName = tableName;
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            var bulkCopyResult = bulkCopyObj.WriteToServer(dataReader);
            recordsAffected = bulkCopyResult.RowsInserted;
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (!isSuccess)
        {
            if (dbContext.Transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteBulkCopyAsync(string tableName, MySqlBulkCopy bulkCopyObj, DbContext dbContext, ITheaConnection connection, IDataReader dataReader, CancellationToken cancellationToken = default)
    {
        var createdAt = DateTime.Now;
        bulkCopyObj.DestinationTableName = tableName;
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            var bulkCopyResult = await bulkCopyObj.WriteToServerAsync(dataReader, cancellationToken);
            recordsAffected = bulkCopyResult.RowsInserted;
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        if (!isSuccess)
        {
            if (dbContext.Transaction == null) await connection.CloseAsync();
            throw exception;
        }
        return recordsAffected;
    }
}