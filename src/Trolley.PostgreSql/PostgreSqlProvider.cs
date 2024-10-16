using Npgsql;
using NpgsqlTypes;
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

namespace Trolley.PostgreSql;

public partial class PostgreSqlProvider : BaseOrmProvider
{
    private readonly static Dictionary<object, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.PostgreSql;
    public override Type NativeDbTypeType => typeof(NpgsqlDbType);
    public override string DefaultTableSchema => "public";
    static PostgreSqlProvider()
    {
        defaultMapTypes[NpgsqlDbType.Bit] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Boolean] = typeof(bool);
        defaultMapTypes[NpgsqlDbType.Smallint] = typeof(short);
        defaultMapTypes[NpgsqlDbType.Integer] = typeof(int);
        defaultMapTypes[NpgsqlDbType.Bigint] = typeof(long);
        defaultMapTypes[NpgsqlDbType.Real] = typeof(float);
        defaultMapTypes[NpgsqlDbType.Double] = typeof(double);
        defaultMapTypes[NpgsqlDbType.Money] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Numeric] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Varchar] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Text] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Json] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Jsonb] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Xml] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[NpgsqlDbType.TimestampTz] = typeof(DateTimeOffset);
#if NET6_0_OR_GREATER
        defaultMapTypes[NpgsqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[NpgsqlDbType.Time] = typeof(TimeOnly);
#else
        defaultMapTypes[NpgsqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[NpgsqlDbType.Time] = typeof(TimeSpan);
#endif
        defaultMapTypes[NpgsqlDbType.Interval] = typeof(TimeSpan);
        defaultMapTypes[NpgsqlDbType.Bytea] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Varbit] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Uuid] = typeof(Guid);
        defaultMapTypes[NpgsqlDbType.Hstore] = typeof(Dictionary<string, string>);

        defaultDbTypes[typeof(bool)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(int)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(float)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(string)] = NpgsqlDbType.Varchar;
        defaultDbTypes[typeof(DateTime)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = NpgsqlDbType.Time;
#endif
        defaultDbTypes[typeof(TimeSpan)] = NpgsqlDbType.Interval;
        defaultDbTypes[typeof(Guid)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        defaultDbTypes[typeof(bool?)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(int?)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong?)] = NpgsqlDbType.Numeric;

        defaultDbTypes[typeof(float?)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double?)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal?)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(DateTime?)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(DateTimeOffset?)] = NpgsqlDbType.TimestampTz;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly?)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = NpgsqlDbType.Time;
#endif
        defaultDbTypes[typeof(TimeSpan?)] = NpgsqlDbType.Interval;
        defaultDbTypes[typeof(Guid?)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        //PostgreSql支持数据类型，值为各自值|int.MinValue
        //如, int[]类型: int.MinValue | NpgsqlDbType.Integer
        defaultDbTypes[typeof(long[])] = NpgsqlDbType.Bigint | NpgsqlDbType.Range;
        defaultDbTypes[typeof(bool[])] = NpgsqlDbType.Boolean | NpgsqlDbType.Range;
        defaultDbTypes[typeof(short[])] = NpgsqlDbType.Smallint | NpgsqlDbType.Range;
        defaultDbTypes[typeof(int[])] = NpgsqlDbType.Integer | NpgsqlDbType.Range;
        defaultDbTypes[typeof(float[])] = NpgsqlDbType.Real | NpgsqlDbType.Range;
        defaultDbTypes[typeof(double[])] = NpgsqlDbType.Double | NpgsqlDbType.Range;
        defaultDbTypes[typeof(decimal[])] = NpgsqlDbType.Numeric | NpgsqlDbType.Range;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly[])] = NpgsqlDbType.DateRange;
        defaultDbTypes[typeof(TimeOnly[])] = NpgsqlDbType.Time | NpgsqlDbType.Range;
#endif
        defaultDbTypes[typeof(TimeSpan[])] = NpgsqlDbType.Interval | NpgsqlDbType.Range;
        defaultDbTypes[typeof(string[])] = NpgsqlDbType.Varchar | NpgsqlDbType.Range;
        defaultDbTypes[typeof(DateTimeOffset[])] = NpgsqlDbType.TimestampTz | NpgsqlDbType.Range;
        defaultDbTypes[typeof(Guid[])] = NpgsqlDbType.Uuid | NpgsqlDbType.Range;

        castTos[typeof(string)] = "VARCHAR";
        castTos[typeof(sbyte)] = "SMALLINT";
        castTos[typeof(byte)] = "SMALLINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "SMALLINT";
        castTos[typeof(int)] = "INTEGER";
        castTos[typeof(uint)] = "INTEGER";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "BIGINT";
        castTos[typeof(float)] = "DECIMAL";
        castTos[typeof(double)] = "DECIMAL";
        castTos[typeof(decimal)] = "DECIMAL";
        castTos[typeof(bool)] = "BOOLEAN";
        castTos[typeof(DateTime)] = "TIMESTAMP";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";
#endif
        castTos[typeof(sbyte?)] = "SMALLINT";
        castTos[typeof(byte?)] = "SMALLINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "SMALLINT";
        castTos[typeof(int?)] = "INTEGER";
        castTos[typeof(uint?)] = "INTEGER";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "BIGINT";
        castTos[typeof(float?)] = "DECIMAL";
        castTos[typeof(double?)] = "DECIMAL";
        castTos[typeof(decimal?)] = "DECIMAL";
        castTos[typeof(bool?)] = "BOOLEAN";
        castTos[typeof(DateTime?)] = "TIMESTAMP";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
#endif
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override ITheaConnection CreateConnection(string dbKey, string connectionString)
        => new PostgreSqlTheaConnection(dbKey, connectionString);
    public override IDbCommand CreateCommand() => new NpgsqlCommand();
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new NpgsqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => new NpgsqlParameter(parameterName, (NpgsqlDbType)nativeDbType) { Value = value };
    public override void ChangeParameter(object dbParameter, Type targetType, object value)
    {
        var fieldValue = Convert.ChangeType(value, targetType);
        var myDbParameter = dbParameter as NpgsqlParameter;
        var nativeDbType = (NpgsqlDbType)this.GetNativeDbType(targetType);
        myDbParameter.NpgsqlDbType = nativeDbType;
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
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的NpgsqlTypes.NpgsqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string CastTo(Type type, object value, string characterSetOrCollation = null)
        => $"CAST({value} AS {castTos[type]})";
    public override string GetIdentitySql(string keyField) => $" RETURNING {keyField}";
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        switch (expectType)
        {
            case Type factType when factType == typeof(bool):
                return Convert.ToBoolean(value) ? "TRUE" : "FALSE";
            case Type factType when factType == typeof(string):
                return $"'{Convert.ToString(value).Replace("'", @"\'")}'";
            case Type factType when factType == typeof(Guid):
                return $"'{value}'::UUID";
            case Type factType when factType == typeof(DateTime):
                return $"TIMESTAMP '{Convert.ToDateTime(value):yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fff}'";
            case Type factType when factType == typeof(DateTimeOffset):
                return $"TIMESTAMPTZ '{(DateTimeOffset)value:yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fffZ}'";
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(DateOnly):
                return $"DATE '{(DateOnly)value:yyyy\\-MM\\-dd}'";
#endif
            case Type factType when factType == typeof(TimeSpan):
                {
                    var factValue = (TimeSpan)value;
                    if (factValue.TotalDays > 1 || factValue.TotalDays < -1)
                        return $"INTERVAL '{(int)factValue.TotalDays}D {factValue:hh\\:mm\\:ss\\.ffffff}'";
                    return $"INTERVAL '{factValue:hh\\:mm\\:ss\\.ffffff}'";
                }
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(TimeOnly): return $"TIME '{(TimeOnly)value:hh\\:mm\\:ss\\.ffffff}'";
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
        var dataType = columnInfo.DataType;
        if (columnInfo.ArrayDimens > 0)
            dataType = dataType.Substring(1);
        NpgsqlDbType result = default;
        switch (dataType)
        {
            case "bool": result = NpgsqlDbType.Boolean; break;

            case "int2": result = NpgsqlDbType.Smallint; break;
            case "int4": result = NpgsqlDbType.Integer; break;
            case "int8": result = NpgsqlDbType.Bigint; break;
            case "float4": result = NpgsqlDbType.Real; break;
            case "float8": result = NpgsqlDbType.Double; break;

            case "numeric": result = NpgsqlDbType.Numeric; break;
            case "money": result = NpgsqlDbType.Money; break;

            case "char":
            case "bpchar": result = NpgsqlDbType.Char; break;
            case "varchar": result = NpgsqlDbType.Varchar; break;
            case "text": result = NpgsqlDbType.Text; break;

            case "date": result = NpgsqlDbType.Date; break;
            case "timestamp": result = NpgsqlDbType.Timestamp; break;
            case "timestamptz": result = NpgsqlDbType.TimestampTz; break;

            case "time": result = NpgsqlDbType.Time; break;
            case "timetz":
                result = NpgsqlDbType.TimeTz; break;
            case "interval": result = NpgsqlDbType.Interval; break;

            case "bit": result = NpgsqlDbType.Bit; break;
            case "bytea":
                result = NpgsqlDbType.Bytea; break;
            case "varbit": result = NpgsqlDbType.Varbit; break;

            case "point": result = NpgsqlDbType.Point; break;
            case "line": result = NpgsqlDbType.Line; break;
            case "lseg": result = NpgsqlDbType.LSeg; break;
            case "box":
                result = NpgsqlDbType.Box; break;
            case "path": result = NpgsqlDbType.Path; break;
            case "polygon": result = NpgsqlDbType.Polygon; break;
            case "circle": result = NpgsqlDbType.Circle; break;

            case "cidr": result = NpgsqlDbType.Cidr; break;
            case "inet": result = NpgsqlDbType.Inet; break;
            case "macaddr": result = NpgsqlDbType.MacAddr; break;

            case "json": result = NpgsqlDbType.Json; break;
            case "jsonb": result = NpgsqlDbType.Jsonb; break;

            case "uuid": result = NpgsqlDbType.Uuid; break;

            case "int4range": result = NpgsqlDbType.Integer | NpgsqlDbType.Range; break;
            case "int8range": result = NpgsqlDbType.Bigint | NpgsqlDbType.Range; break;
            case "numrange": result = NpgsqlDbType.Numeric | NpgsqlDbType.Range; break;
            case "tsrange": result = NpgsqlDbType.Timestamp | NpgsqlDbType.Range; break;
            case "tstzrange": result = NpgsqlDbType.TimestampTz | NpgsqlDbType.Range; break;
            case "daterange": result = NpgsqlDbType.Date | NpgsqlDbType.Range; break;

            case "hstore": result = NpgsqlDbType.Hstore; break;

            case "geometry": result = NpgsqlDbType.Geometry; break;
        }
        if (columnInfo.ArrayDimens > 0)
            result = result | NpgsqlDbType.Range;
        return result;
    }
    public override void MapTables(string connectionString, IEntityMapProvider mapProvider)
    {
        var tableNames = mapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return;
        var sql = @"SELECT b.nspname,a.relname,c.attname,c.attndims,d.typname,CASE WHEN c.atttypmod>0 AND c.atttypmod<32767 THEN c.atttypmod-4 ELSE c.attlen END,e.description,pg_get_expr(g.adbin,g.adrelid),
f.conname IS NOT NULL,h.refobjid IS NOT NULL,c.attnotnull,c.attnum FROM pg_class a INNER JOIN pg_namespace b ON a.relnamespace = b.oid INNER JOIN pg_attribute c ON a.oid = c.attrelid AND c.attnum>0
INNER JOIN pg_type d ON c.atttypid=d.oid LEFT JOIN pg_description e ON e.objoid=c.attrelid AND e.objsubid=c.attnum LEFT JOIN pg_constraint f ON a.oid=f.conrelid AND f.contype='p' and f.conkey @> array[c.attnum] 
LEFT JOIN pg_attrdef g ON a.oid=g.adrelid AND c.attnum=g.adnum LEFT JOIN (select dp.refobjid,dp.refobjsubid FROM pg_depend dp,pg_class cs WHERE dp.objid=cs.oid AND cs.relkind='S') h ON a.oid=h.refobjid
AND c.attnum=h.refobjsubid WHERE a.relkind='r' AND {0} ORDER BY b.nspname,a.relname,c.attnum asc";
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

            sqlBuilder.Append($"b.nspname='{tableBuilder.Key}' AND a.relname IN ({tableBuilder.Value.ToString()})");
        }
        sql = string.Format(sql, sqlBuilder.ToString());
        var entityMappers = mapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        using var connection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand(sql, connection);
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
            var fieldName = reader.ToFieldValue<string>(2);
            var arrayDimens = reader.ToFieldValue<int>(3);
            var dataType = reader.ToFieldValue<string>(4);
            var length = reader.ToFieldValue<int>(5);
            var scale = (length >> 16) & 0xFFFF;
            var precision = length & 0xFFFF;
            var lengthTypes = new[] { "bool", "name", "bit", "varbit", "char", "bpchar", "varchar", "bytea", "text", "uuid" };
            if (length > 0 && !lengthTypes.Contains(dataType))
                length *= 8;
            var needLengthTypes = new[] { "char", "bpchar", "varchar", "bytea", "bit", "varbit" };
            if (dataType == "bpchar")
                dataType = "char";
            var columnType = dataType;
            if (needLengthTypes.Contains(dataType))
                columnType += $"({length})";
            if (arrayDimens > 0)
            {
                sqlBuilder.Clear();
                sqlBuilder.Append(dataType.Substring(1));
                for (int i = 0; i < arrayDimens; i++)
                    sqlBuilder.Append("[]");
                columnType = sqlBuilder.ToString();
            }
            tableInfo.Columns.Add(new DbColumnInfo
            {
                FieldName = fieldName,
                DataType = dataType,
                DbColumnType = columnType,
                MaxLength = length,
                Scale = scale,
                Precision = precision,
                ArrayDimens = arrayDimens,
                Description = reader.ToFieldValue<string>(6),
                DefaultValue = reader.ToFieldValue<string>(7),
                IsPrimaryKey = reader.ToFieldValue<bool>(8),
                IsAutoIncrement = reader.ToFieldValue<bool>(9),
                IsNullable = !reader.ToFieldValue<bool>(10),
                Position = reader.ToFieldValue<int>(11)
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
            case "Excluded":
                var genericArgumentTypes = methodInfo.DeclaringType.GetGenericArguments();
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    cacheKey = RepositoryHelper.GetCacheKey(typeof(IPostgreSqlCreateConflictDoUpdate<>), methodInfo.GetGenericMethodDefinition());
                    //.OnConflict(x => x.UseKeys().Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) }) ... )
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var myVisitor = visitor as PostgreSqlCreateVisitor;
                        if (args[0] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Excluded方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Excluded(f.TotalAmount)}})");
                        if (!myVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{myVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        var fieldName = $"EXCLUDED.{this.GetFieldName(memberMapper.FieldName)}";
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
                cacheKey = RepositoryHelper.GetCacheKey(typeof(Sql), methodInfo.GetGenericMethodDefinition());
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                    var targetArgument = visitor.GetQuotedValue(targetSegment);
                    var rightArgument = visitor.GetQuotedValue(rightSegment);
                    return targetSegment.Merge(rightSegment, $"COALESCE({targetArgument},{rightArgument})", false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
    public int ExecuteBulkCopy(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
        var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        connection.Open();
        var fromMapper = visitor.Tables[0].Mapper;
        int index = 0;
        tableName ??= fromMapper.TableName;
        var builder = new StringBuilder($"COPY {this.GetTableName(tableName)}(");
        foreach ((var refMemberMapper, _) in memberMappers)
        {
            if (index > 0) builder.Append(',');
            builder.Append(this.GetFieldName(refMemberMapper.FieldName));
            index++;
        }
        builder.Append(") FROM STDIN BINARY");
        var dbConnection = connection.BaseConnection as NpgsqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as NpgsqlTransaction;
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
            using var writer = dbConnection.BeginBinaryImport(builder.ToString());
            foreach (var insertObj in insertObjs)
            {
                writer.StartRow();
                foreach ((var refMemberMapper, var valueGetter) in memberMappers)
                {
                    object fieldValue = valueGetter.Invoke(insertObj);
                    writer.Write(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType);
                }
                recordsAffected++;
            }
            writer.Complete();
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
            builder.Clear();
        }
        if (!isSuccess)
        {
            if (transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteBulkCopyAsync(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);
        var dataTable = visitor.ToDataTable(insertObjType, insertObjs, memberMappers, tableName ?? entityMapper.TableName);
        if (dataTable.Rows.Count == 0) return 0;

        await connection.OpenAsync(cancellationToken);
        var fromMapper = visitor.Tables[0].Mapper;
        int index = 0;
        tableName ??= fromMapper.TableName;
        var builder = new StringBuilder($"COPY {this.GetTableName(tableName)}(");
        foreach ((var refMemberMapper, _) in memberMappers)
        {
            if (index > 0) builder.Append(',');
            builder.Append(this.GetFieldName(refMemberMapper.FieldName));
            index++;
        }
        builder.Append(") FROM STDIN BINARY");
        var dbConnection = connection.BaseConnection as NpgsqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as NpgsqlTransaction;
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
            using var writer = await dbConnection.BeginBinaryImportAsync(builder.ToString(), cancellationToken);
            foreach (var insertObj in insertObjs)
            {
                await writer.StartRowAsync(cancellationToken);
                foreach ((var refMemberMapper, var valueGetter) in memberMappers)
                {
                    object fieldValue = valueGetter.Invoke(insertObj);
                    await writer.WriteAsync(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType, cancellationToken);
                }
                recordsAffected++;
            }
            await writer.CompleteAsync(cancellationToken);
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
            builder.Clear();
        }
        if (!isSuccess)
        {
            if (transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
}