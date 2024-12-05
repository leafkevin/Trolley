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
    public string LockName { get; set; }
    public List<string> OutputFieldNames { get; set; }
    public SqlServerCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
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
    public override string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = null;
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var entityMapper = tableSegment.Mapper;
        string tableName;
        if (tableSegment.IsSharding)
            tableName = tableSegment.Body;
        else
        {
            if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _))
                tableName = this.GetShardingTableName();
            else tableName = entityMapper.TableName;
        }
        var tableSchema = tableSegment.TableSchema;
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        tableName = this.OrmProvider.GetTableName(tableName);

        if (this.OutputFieldNames != null && this.IsReturnIdentity)
            throw new NotSupportedException("不支持同时Output、Identity操作，只能选择一种操作");

        string tailSql = null;
        if (this.OutputFieldNames != null)
            tailSql = this.BuildOutputSql(out readerFields);

        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            tailSql = this.OrmProvider.GetIdentitySql(null);
        }
        this.FieldsBuilder.Append(this.ValuesBuilder);
        return $"INSERT INTO {tableName}({this.FieldsBuilder}) VALUES({this.ValuesBuilder}){tailSql}";
    }
    public override string BuildWithBulkSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, _, var firstSqlSetter,
            var loopSqlSetter, var tailSql, readerFields) = this.BuildWithBulk(command);
        Action<string, IEnumerable> executor = null;
        if (tailSql != null)
        {
            executor = (tableName, insertObjs) =>
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;
                }
                builder.Append(tailSql);
            };
        }
        else
        {
            executor = (tableName, insertObjs) =>
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                int index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                    index++;
                }
            };
        }
        if (isNeedSplit)
        {
            var entityType = this.Tables[0].EntityType;
            var tabledInsertObjs = RepositoryHelper.SplitShardingParameters(this.MapProvider, this.ShardingProvider, entityType, insertObjs);
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                executor(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else executor(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
    {
        bool isNeedSplit = false;
        object firstInsertObj = null;
        Type insertObjType = null;
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            insertObjType = entity.GetType();
            break;
        }
        var tableSegment = this.Tables[0];
        var tableName = tableSegment.Mapper.TableName;
        var entityType = tableSegment.EntityType;

        if (tableSegment.IsSharding)
            tableName = tableSegment.Body;
        else isNeedSplit = this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _);

        string fixedSql = "(";
        List<IDbDataParameter> fixedDbParameters = null;
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
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            }
            fixedSql = $"({this.ValuesBuilder}";
        }

        var entityMapper = tableSegment.Mapper;
        var fieldsSetter = this.GetFieldsSetter(entityType, insertObjType);
        var valuesSetter = this.GetValuesSetter(entityType, insertObjType, true);
        var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

        string headSql = "INSERT INTO";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql = $"INSERT INTO {this.OrmProvider.GetTableName(tableSegment.TableSchema)}";

        //生成批量Fields SQL
        fieldsSetter.Invoke(this.FieldsBuilder, this.DbContext, firstInsertObj);

        string tailSql = null;
        List<SqlFieldSegment> readerFields = null;
        if (this.OutputFieldNames != null)
            tailSql = this.BuildOutputSql(out readerFields);
        var fieldsSql = $"({this.FieldsBuilder}) VALUES";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fieldsSql);
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fieldsSql);
            };
        }
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> loopSqlSetter = null;
        loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
        {
            builder.Append(fixedSql);
            typedValuesSetter.Invoke(dbParameters, builder, dbContext, insertObj, suffix);
            builder.Append(')');
        };
        this.DbParameters = command.Parameters;
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, tailSql, readerFields);
    }
    public void WithLock(string lockName) => this.LockName = lockName;
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


    protected virtual string BuildOutputSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = new List<SqlFieldSegment>();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" OUTPUT ");
        for (int i = 0; i < this.OutputFieldNames.Count; i++)
        {
            var fieldName = this.OutputFieldNames[i];
            if (i > 0) builder.Append(',');
            builder.Append($"INSERTED.{fieldName}");

            if (fieldName == "*")
            {
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;
                    readerFields.Add(new SqlFieldSegment
                    {
                        FieldType = SqlFieldType.Field,
                        FromMember = memberMapper.Member,
                        TargetMember = memberMapper.Member,
                        SegmentType = memberMapper.MemberType,
                        NativeDbType = memberMapper.NativeDbType,
                        TypeHandler = memberMapper.TypeHandler,
                        Body = memberMapper.FieldName
                    });
                }
            }
            else
            {
                var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
                readerFields.Add(new SqlFieldSegment
                {
                    FieldType = SqlFieldType.Field,
                    FromMember = memberMapper.Member,
                    TargetMember = memberMapper.Member,
                    SegmentType = memberMapper.MemberType,
                    NativeDbType = memberMapper.NativeDbType,
                    TypeHandler = memberMapper.TypeHandler,
                    Body = memberMapper.FieldName
                });
            }
        }
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.LockName = null;
        this.OutputFieldNames = null;
    }
}