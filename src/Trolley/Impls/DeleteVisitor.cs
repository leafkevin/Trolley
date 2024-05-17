using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class DeleteVisitor : SqlVisitor, IDeleteVisitor
{
    private List<CommandSegment> deferredSegments = new();
    public DeleteVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
    {
        this.DbKey = dbKey;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.ShardingProvider = shardingProvider;
        this.IsParameterized = isParameterized;
        this.TableAsStart = tableAsStart;
        this.ParameterPrefix = parameterPrefix;
    }
    public virtual void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true)
    {
        if (!isMultiple)
        {
            this.Tables = new();
            this.Tables.Add(new TableSegment
            {
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            });
        }
        if (!isFirst) this.Clear();
    }
    public virtual string BuildCommand(IDbCommand command)
    {
        string sql = null;
        this.DbParameters = command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "WhereWith":
                    sql = this.VisitWhereWith(command, deferredSegment.Value);
                    break;
                case "Where":
                    this.VisitWhere(deferredSegment.Value as Expression);
                    break;
                case "And":
                    this.VisitAnd(deferredSegment.Value as Expression);
                    break;
            }
        }
        if (sql == null)
        {
            var entityMapper = this.Tables[0].Mapper;
            var entityTableName = this.OrmProvider.GetTableName(entityMapper.TableName);
            var builder = new StringBuilder($"DELETE FROM {entityTableName}");
            if (!string.IsNullOrEmpty(this.WhereSql))
                builder.Append(" WHERE " + this.WhereSql);
            sql = builder.ToString();
        }
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Delete,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments,
            Tables = this.Tables,
            RefQueries = this.RefQueries,
            IsNeedTableAlias = this.IsNeedTableAlias
        };
    }
    public virtual void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        this.Tables = multiCommand.Tables;
        this.RefQueries = multiCommand.RefQueries;
        this.IsNeedTableAlias = multiCommand.IsNeedTableAlias;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command));
    }
    public virtual IDeleteVisitor WhereWith(object wherKeys)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WhereWith",
            Value = wherKeys
        });
        return this;
    }
    public virtual IDeleteVisitor Where(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Where",
            Value = whereExpr
        });
        return this;
    }
    public virtual IDeleteVisitor And(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "And",
            Value = whereExpr
        });
        return this;
    }

    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
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
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return sqlSegment.Next(memberExpr.Expression);
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return sqlSegment.Next(memberExpr.Expression);
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.OrderNo.Length==10 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                return formatter.Invoke(this, targetSegment);
            }

            if (memberExpr.IsParameter(out _))
            {
                //Where(f=>... && f.Amount>5 && ...)
                //Include(f=>f.Buyer); 或是 IncludeMany(f=>f.Orders)
                //Select(f=>new {f.OrderId, ...})
                //Where(f=>f.Order.Id>10)
                //Include(f=>f.Order.Buyer)
                //Select(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>f.Order.OrderId)
                //OrderBy(f=>new {f.Order.OrderId, ...})
                //OrderBy(f=>f.Order.OrderId)                
                var memberMapper = this.Tables[0].Mapper.GetMemberMap(memberExpr.Member.Name);
                if (memberMapper.IsIgnore)
                    throw new Exception($"类{this.Tables[0].EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{this.Tables[0].EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                sqlSegment.HasField = true;
                sqlSegment.IsConstant = false;
                sqlSegment.TableSegment = this.Tables[0];
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.MemberMapper = memberMapper;
                sqlSegment.SegmentType = memberMapper.MemberType;
                if (memberMapper.UnderlyingType.IsEnum)
                    sqlSegment.ExpectType = memberMapper.UnderlyingType;
                sqlSegment.NativeDbType = memberMapper.NativeDbType;
                sqlSegment.TypeHandler = memberMapper.TypeHandler;
                sqlSegment.Value = fieldName;
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        //这里不做参数化，后面统一走参数化处理
        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        return sqlSegment;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
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
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
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
        this.WhereSql = null;
        this.IsFromQuery = false;
        this.TableAsStart = 'a';
        this.IsNeedTableAlias = false;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
    }
    protected virtual string VisitWhereWith(IDbCommand command, object whereKeys)
    {
        string sql = null;
        var entityType = this.Tables[0].EntityType;
        var isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
        if (isBulk)
        {
            int index = 0;
            var builder = new StringBuilder();
            (var isMultiKeys, var headSql, var commandInitializer) = RepositoryHelper.BuildDeleteBulkCommandInitializer(this.OrmProvider, this.MapProvider, entityType, whereKeys);
            var entities = whereKeys as IEnumerable;
            if (this.IsMultiple)
            {
                if (isMultiKeys)
                {
                    foreach (var entity in entities)
                    {
                        if (index > 0) builder.Append(';');
                        commandInitializer.Invoke(command.Parameters, this.OrmProvider, builder, entity, $"_m{this.CommandIndex}_{index}");
                        index++;
                    }
                }
                else
                {
                    builder.Append(headSql);
                    foreach (var entity in entities)
                    {
                        if (index > 0) builder.Append(',');
                        commandInitializer.Invoke(command.Parameters, this.OrmProvider, builder, entity, $"_m{index}");
                        index++;
                    }
                    builder.Append(')');
                }
            }
            else
            {
                if (isMultiKeys)
                {
                    foreach (var entity in entities)
                    {
                        if (index > 0) builder.Append(';');
                        commandInitializer.Invoke(command.Parameters, this.OrmProvider, builder, entity, $"{index}");
                        index++;
                    }
                }
                else
                {
                    builder.Append(headSql);
                    foreach (var entity in entities)
                    {
                        if (index > 0) builder.Append(',');
                        commandInitializer.Invoke(command.Parameters, this.OrmProvider, builder, entity, $"{index}");
                        index++;
                    }
                    builder.Append(')');
                }
            }
            sql = builder.ToString();
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.OrmProvider, this.MapProvider, entityType, whereKeys, this.IsMultiple);
            if (this.IsMultiple)
            {
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string, string>;
                sql = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereKeys, $"_m{this.CommandIndex}");
            }
            else
            {
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
                sql = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereKeys);
            }
        }
        return sql;
    }
    protected virtual void VisitWhere(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.LastWhereNodeType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
    }
    protected virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        if (this.LastWhereNodeType == OperationType.Or)
        {
            this.WhereSql = $"({this.WhereSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND " + conditionSql;
        else this.WhereSql = conditionSql;
        this.IsWhere = false;
    }
    private void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder builder)
    {
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        if (builder.Length > 0)
            builder.Append(',');
        builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName) + "=");
        if (sqlSegment == SqlSegment.Null)
            builder.Append("NULL");
        else builder.Append(this.GetQuotedValue(sqlSegment));
    }
}