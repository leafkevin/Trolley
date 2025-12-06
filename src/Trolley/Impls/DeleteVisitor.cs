using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class DeleteVisitor : SqlVisitor, IDeleteVisitor
{
    public bool IsWhereKeys { get; set; }
    protected List<CommandSegment> deferredSegments = new();

    public bool HasWhere { get; protected set; }

    public DeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
        this.Tables = new()
        {
            new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            }
        };
        if (this.TryGetTableShardingInfo(entityType, TableShardingUsageMode.WriteOnly, out var tableShardingInfo))
            this.Tables[0].TableShardingInfo = tableShardingInfo;
    }
    public virtual string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.DbParameters = command.Parameters;

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                if (!string.IsNullOrEmpty(tableSegment.Body))
                {
                    shardingTables = tableSegment.Body;
                    shardingType = ShardingTableType.SingleTable;
                }
                else
                {
                    shardingTables = tableSegment.TableNames;
                    shardingType = ShardingTableType.MultiTable;
                }
            }
            else throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，但未指定分表，原始表：{tableSegment.Mapper.TableName}");
        }
        if (this.IsWhereKeys)
        {
            var whereKeys = this.deferredSegments[0].Value;
            var isBulk = this.ActionMode == ActionMode.Bulk;

            switch (this.ActionMode)
            {
                case ActionMode.Bulk:
                    break;
                default:
                    throw new NotSupportedException($"不支持的ActionMode类型：{this.ActionMode}");
            }
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, whereKeys, tableSegment.IsSharding, isBulk, false);

            int index = 0;
            var builder = new StringBuilder();
            var whereSqlBuilder = new StringBuilder();
            Action sqlExecuter = null;
            if (isBulk)
            {
                var typedWhereSqlSetter = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";
                sqlExecuter = () =>
                {
                    var jointMark = isMultiKeys ? " OR " : ",";
                    foreach (var entity in entities)
                    {
                        if (index > 0) whereSqlBuilder.Append(jointMark);
                        typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, suffixGetter.Invoke(index));
                        index++;
                    }
                    if (!isMultiKeys) whereSqlBuilder.Append(')');
                };
            }
            else
            {
                if (this.IsMultiple)
                {
                    var typedWhereSqlSetter = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                    sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys, $"_m{this.CommandIndex}");
                }
                else
                {
                    var typedWhereSqlSetter = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
                    sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
                }
            }
            if (!string.IsNullOrEmpty(this.Tables[0].TableSchema))
                headSqlSetter = (builder, tableName) => headSqlSetter.Invoke(builder, this.Tables[0].TableSchema + "." + tableName);
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableNames = this.ShardingTables[0].TableNames;
                sqlExecuter.Invoke();
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    headSqlSetter.Invoke(builder, tableNames[i]);
                    builder.Append(whereSqlBuilder);
                }
            }
            else
            {
                sqlExecuter.Invoke();
                headSqlSetter.Invoke(builder, this.Tables[0].Body ?? origName);
                builder.Append(whereSqlBuilder);
            }
            sql = builder.ToString();
            builder.Clear();
            whereSqlBuilder.Clear();
        }
        else
        {
            if (this.HasWhere) this.WhereBuilder = new();
            Func<IDataParameterCollection, DbContext, object, string> commandInitializer = null;
            foreach (var deferredSegment in this.deferredSegments)
            {
                switch (deferredSegment.Type)
                {
                    case "WhereBy":
                        commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, deferredSegment.Value, false, false, false);
                        this.VisitWhereSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "WhereById":
                        commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, deferredSegment.Value, true, false, false);
                        this.VisitWhereSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "WhereByIds":
                        commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, deferredSegment.Value, true, false, true);
                        this.VisitWhereSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "Where":
                    case "And":
                        this.VisitAnd(deferredSegment.Value as Expression);
                        break;
                    case "Or":
                        this.VisitOr(deferredSegment.Value as Expression);
                        break;
                }
            }

            var builder = new StringBuilder();
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableSegment = this.ShardingTables[0];
                var tableNames = tableSegment.TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    builder.Append("DELETE FROM ");
                    builder.Append(this.OrmProvider.GetTableName(tableNames[i]));
                    builder.Append(" WHERE ");
                    builder.Append(this.WhereBuilder.ToString());
                }
            }
            else
            {
                var tableName = this.Tables[0].Body ?? this.Tables[0].Mapper.TableName;
                builder.Append($"DELETE FROM {this.OrmProvider.GetTableName(tableName)} WHERE {this.WhereBuilder.ToString()}");
            }
            sql = builder.ToString();
        }
        return sql;
    }
    public virtual void WhereBy(object whereObj)
    {
        this.IsWhereKeys = true;
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereBy",
            Value = whereObj
        });
    }
    public virtual void WhereById(object whereKey)
    {
        this.IsWhereKeys = true;
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereById",
            Value = whereKey
        });
    }
    public virtual void WhereByIds(IEnumerable whereKeys)
    {
        this.IsWhereKeys = true;
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereByIds",
            Value = whereKeys
        });
    }
    public virtual void And(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "And",
            Value = whereExpr
        });
    }
    public virtual void Or(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Or",
            Value = whereExpr
        });
    }
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                sqlSegment.SegmentType = memberExpr.Type;
                return sqlSegment;
            }

            if (memberExpr.IsParameter(out _))
            {
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.Tables[0];
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                if (memberMapper.IsIgnore)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                sqlSegment.HasField = true;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.SegmentType = memberMapper.MemberType;
                if (memberMapper.UnderlyingType.IsEnum)
                    sqlSegment.ExpectType = memberMapper.UnderlyingType;
                sqlSegment.NativeDbType = memberMapper.NativeDbType;
                sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
                sqlSegment.TypeHandler = memberMapper.TypeHandler;
                sqlSegment.Body = fieldName;
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlFieldSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
            return sqlSegment;
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        //这里不做参数化，后面统一走参数化处理
        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.SegmentType = memberExpr.Type;
        return sqlSegment;
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var builder = new StringBuilder();
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;
                this.AddMemberElement(sqlSegment.Next(newExpr.Arguments[i]), memberMapper, builder);
            }
            return sqlSegment.Change(builder.ToString());
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            this.AddMemberElement(sqlSegment.Next(memberAssignment.Expression), memberMapper, builder);
        }
        return sqlSegment.Change(builder.ToString());
    }
    public virtual void Clear()
    {
        this.Tables?.Clear();
        this.TableAliases?.Clear();
        this.ReaderFields?.Clear();
        this.WhereBuilder = null;
        this.IsFromQuery = false;
        this.TableAsStart = 'a';
        this.IsNeedTableAlias = false;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
    }
    public virtual void VisitWhereBy(object whereObj)
    {
        var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereObj, false, false, false);
        var conditionSql = commandInitializer.Invoke(this.DbParameters, this.DbContext, whereObj);
        if (this.LastWhereOperationType == OperationType.Or)
        {
            this.WhereBuilder.Insert(0, '(');
            this.WhereBuilder.Append(')');
        }
        if (this.LastWhereOperationType != OperationType.None)
            this.WhereBuilder.Append(" AND ");
        this.WhereBuilder.Append(conditionSql);
        this.LastWhereOperationType = OperationType.And;
    }
    public void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper, StringBuilder builder)
    {
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        if (builder.Length > 0)
            builder.Append(',');
        builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName) + "=");
        if (sqlSegment == SqlFieldSegment.Null)
            builder.Append("NULL");
        else builder.Append(this.GetQuotedValue(sqlSegment));
    }
}