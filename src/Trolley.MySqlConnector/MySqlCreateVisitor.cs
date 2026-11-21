using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlCreateVisitor : CreateVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    public bool IsUseSetAlias { get; set; }
    public string RowAlias { get; set; } = "newRow";

    public MySqlCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        : base(entityType, dbContext, tableAsStart, command) { }

    public virtual void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public virtual void Returning(string fieldNames)
    {
        this.ReaderFields = new();
        this.OutputSql = $" RETURNING {fieldNames}";
        var entityType = this.Tables[0].EntityType;
        if (fieldNames == "*")
        {
            var entityMapper = this.Tables[0].Mapper;
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                this.ReaderFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TargetMember = memberMapper.Member,
                    ReaderType = memberMapper.MemberType,
                    MemberMapper = memberMapper,
                    MemberName = memberMapper.FieldName,
                    Value = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.RawSql,
                Value = fieldNames
            });
        }
    }
    public virtual void Returning(LambdaExpression fieldsSelector)
    {
        this.ReaderFields = new();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" RETURNING ");
        this.InitTableAlias(fieldsSelector);
        switch (fieldsSelector.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = fieldsSelector.Body as MemberExpression;
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberExpr });
                    var fieldName = this.WrapSql(sqlSegment);
                    builder.Append(fieldName);
                    var isNeedAlias = false;
                    if (sqlSegment.MemberName != memberExpr.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberExpr.Member,
                        ReaderType = memberExpr.Type,
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper?.FieldName,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.Visit(new SqlSegment { Expression = newExpr.Arguments[i] });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberInfo.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberInfo,
                        ReaderType = memberInfo.GetMemberType(),
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper.FieldName,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberAssignment.Expression });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberAssignment.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberAssignment.Member,
                        ReaderType = memberAssignment.Member.GetMemberType(),
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper?.FieldName,
                        //MappedTargetType = sqlSegment.MappedTargetType,
                        //TypeHandler = sqlSegment.TypeHandler,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.Parameter:
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;

                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberMapper.Member,
                        ReaderType = memberMapper.MemberType,
                        MemberMapper = memberMapper,
                        MemberName = memberMapper.FieldName,
                        //MappedTargetType = memberMapper.MappedTargetType,
                        //TypeHandler = memberMapper.TypeHandler,
                        Value = memberMapper.FieldName
                    });
                }
                builder.Append('*');
                break;
            default:
                {
                    var sqlSegment = this.Visit(new SqlSegment { Expression = fieldsSelector });
                    for (int i = 0; i < this.ReaderFields.Count; i++)
                    {
                        var readerField = this.ReaderFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(readerField.Value);
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        ReaderType = fieldsSelector.Type,
                        Value = sqlSegment.Value
                    });
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
    }
    public (ShardingTableType, object, IEnumerable, int?, List<MemberMap>, List<Func<object, object>>) BuildWithBulkCopy()
    {
        (var insertObjs, int? timeoutSeconds) = ((IEnumerable, int?))this.deferredSegments[0].Value;
        object firstInsertObj = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();
        var tableSegment = this.Tables[0];

        var shardingType = tableSegment.ShardingType;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                if (!string.IsNullOrEmpty(tableSegment.Body))
                    shardingTables = tableSegment.Body;
                else if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 0)
                {
                    var entityType = tableSegment.EntityType;
                    throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，数据插入不能设置多个分表，原始表：{tableSegment.Mapper.TableName}");
                }
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, TableUsageMode.WriteOnly, insertObjType, insertObjs, firstInsertObj, this.ShardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(insertObjType, tableSegment.Mapper, firstInsertObj, false);
        return (shardingType, shardingTables, insertObjs, timeoutSeconds, memberMappers, valueGetters);
    }
    public override string BuildHeadSql()
    {
        if (this.IsUseIgnoreInto) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var mySqlSegment = base.VisitMemberAccess(sqlSegment);
        if (this.IsUseSetAlias)
            mySqlSegment.Value = $"{this.RowAlias}.{mySqlSegment.Value}";
        return mySqlSegment;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.RowAlias = null;
        this.FromSql = null;
        this.OutputSql = null;
    }
}