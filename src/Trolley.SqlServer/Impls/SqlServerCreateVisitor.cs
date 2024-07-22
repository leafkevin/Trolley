using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerCreateVisitor : CreateVisitor, ICreateVisitor
{
    public List<string> OutputFieldNames { get; set; }
    public SqlServerCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<ReaderField> readerFields)
    {
        string sql = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildWithBulkSql(command, out readerFields);
        else
        {
            this.DbParameters ??= command.Parameters;
            foreach (var deferredSegment in this.deferredSegments)
            {
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public override string BuildSql(out List<ReaderField> readerFields)
    {
        var entityType = this.Tables[0].EntityType;
        var entityMapper = this.Tables[0].Mapper;
        var tableName = entityMapper.TableName;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            if (string.IsNullOrEmpty(this.Tables[0].Body))
                throw new Exception($"实体表{entityType.FullName}有配置分表，当前操作未指定分表，请调用UseTable或UseTableBy方法指定分表");
            tableName = this.Tables[0].Body;
        }
        tableName = this.OrmProvider.GetTableName(tableName);

        var builder = new StringBuilder($"INSERT INTO {tableName} (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0) builder.Append(',');
            builder.Append(insertField.Fields);
        }
        builder.Append(')');
        string outputSql = null;
        readerFields = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
        {
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();
            builder.Append(outputSql);
        }
        builder.Append(" VALUES (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0) builder.Append(',');
            builder.Append(insertField.Values);
        }
        builder.Append(')');
        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrement)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            builder.Append(this.OrmProvider.GetIdentitySql(null));
        }
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildShardingTablesSql(string tableSchema)
    {
        var count = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable).Count;
        var builder = new StringBuilder($"SELECT name FROM sys.sysobjects WHERE xtype='U' AND ");
        if (count > 1)
        {
            builder.Append('(');
            int index = 0;
            foreach (var tableSegment in this.ShardingTables)
            {
                if (tableSegment.ShardingType > ShardingTableType.MultiTable)
                {
                    if (index > 0) builder.Append(" OR ");
                    builder.Append($"name LIKE '{tableSegment.Mapper.TableName}%'");
                    index++;
                }
            }
            builder.Append(')');
        }
        else
        {
            if (this.ShardingTables.Count > 1)
            {
                var tableSegment = this.ShardingTables.Find(f => f.ShardingType > ShardingTableType.MultiTable);
                builder.Append($"name LIKE '{tableSegment.Mapper.TableName}%'");
            }
            else builder.Append($"name LIKE '{this.ShardingTables[0].Mapper.TableName}%'");
        }
        return builder.ToString();
    }
    public override (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<ReaderField>) BuildWithBulk(IDbCommand command)
    {
        bool isNeedSplit = false;
        object firstInsertObj = null;
        Type insertObjType = null;

        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            break;
        }
        insertObjType = firstInsertObj.GetType();
        var tableName = this.Tables[0].Mapper.TableName;
        var entityType = this.Tables[0].EntityType;

        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            //有设置分表，优先使用分表，没有设置分表，则根据数据的字段确定分表
            if (!string.IsNullOrEmpty(this.Tables[0].Body))
                tableName = this.Tables[0].Body;
            //未指定分表，需要根据数据字段确定分表
            else isNeedSplit = true;
        }
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);
        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object, string> loopSqlSetter = null;

        string outputSql = null;
        List<ReaderField> readerFields = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();

        if (this.deferredSegments.Count > 1)
        {
            this.DbParameters = new TheaDbParameterCollection();
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields操作");
                }
            }

            var fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, List<MemberMap>, object, string>;

                var builder = new StringBuilder();
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Fields);
                }
                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(')');
                if (outputSql != null)
                    builder.Append(outputSql);
                builder.Append(" VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                    builder.Append(firstHeadSql);
                    if (fixedDbParameters.Count > 0)
                        fixedDbParameters.ForEach(f => dbParameters.Add(f));
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    for (int i = 0; i < this.InsertFields.Count; i++)
                    {
                        var insertField = this.InsertFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(insertField.Values);
                    }
                    var entityMapper = this.Tables[0].Mapper;
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                    for (int i = 0; i < this.InsertFields.Count; i++)
                    {
                        var insertField = this.InsertFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(insertField.Fields);
                    }
                    typedFieldsSqlPartSetter.Invoke(builder);
                    builder.Append(')');
                    if (outputSql != null)
                        builder.Append(outputSql);
                    builder.Append(" VALUES ");
                    if (fixedDbParameters.Count > 0)
                        fixedDbParameters.ForEach(f => dbParameters.Add(f));
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    for (int i = 0; i < this.InsertFields.Count; i++)
                    {
                        var insertField = this.InsertFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(insertField.Values);
                    }
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj, suffix);
                    builder.Append(')');
                };
            }
            this.DbParameters = command.Parameters;
        }
        else
        {
            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, List<MemberMap>, object, string>;

                var builder = new StringBuilder();
                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(')');
                if (outputSql != null)
                    builder.Append(outputSql);
                builder.Append(" VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                    builder.Append(firstHeadSql);
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    var entityMapper = this.Tables[0].Mapper;
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                    typedFieldsSqlPartSetter.Invoke(builder);
                    builder.Append(')');
                    if (outputSql != null)
                        builder.Append(outputSql);
                    builder.Append(" VALUES ");
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj, suffix);
                    builder.Append(')');
                };
            }
        }
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, readerFields);
    }

    public void Output(params string[] fieldNames)
    {
        this.OutputFieldNames ??= new();
        this.OutputFieldNames.AddRange(fieldNames);
    }
    public virtual void Output(Expression fieldsSelector)
        => this.OutputFieldNames = this.VisitFields(fieldsSelector);
    public void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;

    private (string, List<ReaderField>) BuildOutputSqlReaderFields()
    {
        var readerFields = new List<ReaderField>();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder();
        Action<MemberMap> addReaderField = memberMapper =>
        {
            readerFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                FromMember = memberMapper.Member,
                TargetMember = memberMapper.Member,
                TargetType = memberMapper.MemberType,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = memberMapper.FieldName
            });
        };
        builder.Append(" OUTPUT ");
        for (int i = 0; i < this.OutputFieldNames.Count; i++)
        {
            var fieldName = this.OutputFieldNames[i];
            if (i > 0) builder.Append(',');
            builder.Append($"INSERTED.{fieldName}");

            if (fieldName == "*")
            {
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    addReaderField.Invoke(memberMapper);
                }
            }
            else
            {
                var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
                addReaderField.Invoke(memberMapper);
            }
        }
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return (sql, readerFields);
    }
}
