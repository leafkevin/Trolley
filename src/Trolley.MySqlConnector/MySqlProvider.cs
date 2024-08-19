using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.MySqlConnector;

public partial class MySqlProvider : BaseOrmProvider
{
    private readonly static Dictionary<object, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.MySql;
    public override Type NativeDbTypeType => typeof(MySqlDbType);

    static MySqlProvider()
    {
        defaultMapTypes[MySqlDbType.Bit] = typeof(bool);
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
        defaultMapTypes[MySqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[MySqlDbType.Time] = typeof(TimeOnly);
        defaultMapTypes[MySqlDbType.TinyBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.MediumBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.LongBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Blob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Binary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.VarBinary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Guid] = typeof(Guid);
        defaultMapTypes[MySqlDbType.Enum] = typeof(string);

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
        defaultDbTypes[typeof(DateTimeOffset)] = MySqlDbType.Timestamp;
        defaultDbTypes[typeof(DateOnly)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = MySqlDbType.Time;
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
        defaultDbTypes[typeof(DateTimeOffset?)] = MySqlDbType.Timestamp;
        defaultDbTypes[typeof(DateOnly?)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = MySqlDbType.Time;
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
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";

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
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
    }
    public override IDbConnection CreateConnection(string connectionString)
        => new MySqlConnection(connectionString);
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
    public override string GetTableName(string tableName) => "`" + tableName + "`";
    public override string GetFieldName(string fieldName) => "`" + fieldName + "`";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的MySqlConnector.MySqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string CastTo(Type type, object value)
        => $"CAST({value} AS {castTos[type]})";
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
    public override void MapTables(string connectionString, IEntityMapProvider mapProvider)
    {
        var tableNames = mapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return;
        var sql = @"SELECT a.TABLE_SCHEMA,a.TABLE_NAME,a.COLUMN_NAME,a.DATA_TYPE,a.COLUMN_TYPE,a.CHARACTER_MAXIMUM_LENGTH,a.NUMERIC_SCALE,a.NUMERIC_PRECISION,a.COLUMN_COMMENT,a.COLUMN_DEFAULT,
		a.COLUMN_KEY='PRI',INSTR(IFNULL(a.EXTRA,''),'auto_increment'),a.IS_NULLABLE='YES',a.ORDINAL_POSITION FROM INFORMATION_SCHEMA.COLUMNS a WHERE {0} ORDER BY a.TABLE_SCHEMA,a.TABLE_NAME,a.ORDINAL_POSITION";

        using var connection = new MySqlConnection(connectionString);
        using var command = new MySqlCommand(sql, connection);
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
                    tableBuilders.TryAdd(tableSchema, builder = new StringBuilder());
            }
            else
            {
                var tableSchema = connection.Database;
                if (!tableBuilders.TryGetValue(tableSchema, out builder))
                    tableBuilders.TryAdd(tableSchema, builder = new StringBuilder());
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
        var entityMappers = mapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        command.CommandText = sql;
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        DbTableInfo tableInfo = null;
        while (reader.Read())
        {
            var tableSchema = reader.GetString(0);
            var tableName = reader.GetString(1);
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
                FieldName = reader.GetString(2),
                DataType = reader.GetString(3),
                DbColumnType = reader.GetString(4),
                MaxLength = (int)reader.ToValue<ulong>(5),
                Scale = reader.ToValue<int>(6),
                Precision = reader.ToValue<int>(7),
                Description = reader.ToValue<string>(8),
                DefaultValue = reader.ToValue<string>(9),
                IsPrimaryKey = reader.ToValue<bool>(10),
                IsAutoIncrement = reader.ToValue<bool>(11),
                IsNullable = reader.ToValue<bool>(12),
                Position = reader.ToValue<int>(13)
            });
        }
        reader.Close();
        connection.Close();

        var fieldMapHandler = mapProvider.FieldMapHandler;
        foreach (var entityMapper in entityMappers)
        {
            (var tableSchema, var tableName) = this.GetFullTableName(entityMapper.TableName);
            tableSchema ??= connection.Database;
            tableInfo = tableInfos.Find(f => f.TableSchema == tableSchema && f.TableName == tableName);
            if (tableInfo == null)
                continue;

            var memberInfos = entityMapper.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            foreach (var columnInfo in tableInfo.Columns)
            {
                //数据库字段没有映射到实体成员,IsRowVersion
                if (!fieldMapHandler.TryFindMember(columnInfo.FieldName, memberInfos, out var memberInfo))
                    continue;

                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                {
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
                else
                {
                    memberMapper.DbColumnType = columnInfo.DbColumnType;
                    memberMapper.IsKey = columnInfo.IsPrimaryKey;
                    memberMapper.IsAutoIncrement = columnInfo.IsAutoIncrement;
                    memberMapper.IsRequired = !columnInfo.IsNullable;
                    memberMapper.MaxLength = columnInfo.MaxLength;
                    memberMapper.NativeDbType = this.MapNativeDbType(columnInfo);
                    memberMapper.Position = columnInfo.Position;
                }
                //实体类类型成员
                if ((memberMapper.UnderlyingType.IsClass && memberMapper.UnderlyingType != typeof(string)
                    || memberMapper.UnderlyingType.IsEntityType(out _))
                    && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string))
                {
                    memberMapper.TypeHandlerType = typeof(JsonTypeHandler);
                    memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
                //object类型
                if (memberMapper.MemberType == typeof(object) && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string))
                {
                    memberMapper.TypeHandlerType = typeof(ToStringTypeHandler);
                    memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
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
            case "Values":
                var genericArgumentTypes = methodInfo.DeclaringType.GetGenericArguments();
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IMySqlCreateDuplicateKeyUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    cacheKey = HashCode.Combine(typeof(IMySqlCreateDuplicateKeyUpdate<>), methodInfo.GetGenericMethodDefinition());
                    //.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var myVisitor = visitor as MySqlCreateVisitor;
                        if (args[0] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Values方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Values(f.TotalAmount)}})");
                        if (!myVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{myVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        //使用别名，一定要先使用，后使用的话，存在表达式计算场景无法解析，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                        var fieldName = this.GetFieldName(memberMapper.FieldName);
                        if (myVisitor.IsUseSetAlias) fieldName = myVisitor.SetRowAlias + "." + fieldName;
                        else fieldName = $"VALUES({fieldName})";
                        return new SqlFieldSegment
                        {
                            HasField = true,
                            FromMember = memberMapper.Member,
                            NativeDbType = memberMapper.NativeDbType,
                            TypeHandler = memberMapper.TypeHandler,
                            Body = fieldName
                        };
                    });
                    return true;
                }
                break;
            case "IsNull":
                cacheKey = HashCode.Combine(typeof(Sql), methodInfo.GetGenericMethodDefinition());
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
}
