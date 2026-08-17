using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class DeleteVisitor : SqlVisitor, IDeleteVisitor
{
    protected List<CommandSegment> deferredSegments = new();

    public bool HasWhere { get; protected set; }
    public string OutputSql { get; set; }

    public DeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
    {
        this.DbContext = dbContext;
        this.TableAliasStart = tableAsStart;
        this.Command = command;
        if (command != null)
        {
            this.Connection = command.Connection;
            this.DbParameters = command.Parameters;
        }
        this.Tables = new()
        {
            new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.EntityMapProvider.GetEntityMap(entityType)
            }
        };
        if (this.TryGetTableShardingInfo(entityType, TableShardingUsageMode.WriteOnly, out var tableShardingInfo))
            this.Tables[0].TableShardingInfo = tableShardingInfo;
    }
    public virtual string BuildSql(ITheaCommand command, out List<ReaderField> readerFields)
    {
        readerFields = null;
        this.DbParameters = command.Parameters;

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding)
            throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，但未指定分表，请使用UseTable/UseTableBy/UseTableByRange方法手动指定分表，原始表：{tableSegment.Mapper.TableName}");

        if (this.HasWhere) this.WhereBuilder = new();
        Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "AndBy":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "AndById":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "AndByIds":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "And":
                    this.VisitAnd(deferredSegment.Value as Expression);
                    break;
                case "OrBy":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "OrById":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "OrByIds":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "Or":
                    this.VisitOr(deferredSegment.Value as Expression);
                    break;
            }
        }
        var whereSql = this.WhereBuilder.ToString();
        this.WhereBuilder.Clear();

        var builder = this.WhereBuilder;
        builder.Append($"DELETE FROM {this.GetFormatTableName(tableSegment)}");
        if (this.HasWhere) builder.Append($" WHERE {whereSql}");
        if (!string.IsNullOrEmpty(this.OutputSql))
            builder.Append(this.OutputSql);

        var sql = builder.ToString();
        if (tableSegment.ShardingType > ShardingTableType.SingleTable)
        {
            builder.Clear();
            var tableNames = tableSegment.TableNames;
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (i > 0) builder.Append(';');
                builder.Append(sql);
            }
            sql = builder.ToString();
        }
        builder.Clear();
        return sql;
    }
    public virtual void AndBy(object whereObj)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "AndBy",
            Value = whereObj
        });
    }
    public virtual void AndById(object whereKey)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "AndById",
            Value = whereKey
        });
    }
    public virtual void AndByIds(IEnumerable whereKeys)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "AndByIds",
            Value = whereKeys
        });
    }
    public virtual void And(Expression whereExpr)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "And",
            Value = whereExpr
        });
    }
    public virtual void OrBy(object whereObj)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OrBy",
            Value = whereObj
        });
    }
    public virtual void OrById(object whereKey)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OrById",
            Value = whereKey
        });
    }
    public virtual void OrByIds(IEnumerable whereKeys)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OrByIds",
            Value = whereKeys
        });
    }
    public virtual void Or(Expression whereExpr)
    {
        this.HasWhere = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "Or",
            Value = whereExpr
        });
    }
    public virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitAndSql(whereSql, operationType);
    }
    public virtual void VisitOr(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitOrSql(whereSql, operationType);
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        //Select场景，实体成员访问，返回ReaderField实体类型，ReaderFields并且有值，子ReaderFields的Body可无值
        //Select场景和Where场景，单个字段成员访(包括Json实体类型字段)，返回FromMember，TargetMember，字段类型，Body有值为带有别名的FieldName
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;

        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (memberExpr.Type.IsValueType && Nullable.GetUnderlyingType(memberExpr.Type) != null)
            {
                if (memberInfo.Name == "HasValue")
                {
                    sqlSegment.Push(DeferredOperation.IsNull);
                    sqlSegment.Push(DeferredOperation.Not);
                }
                return this.Visit(sqlSegment.Next(memberExpr.Expression));
            }

            //各种OrmProvider提供的类型实例成员访问，如：DateTime,TimeSpan,String.Length
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                //sqlSegment.TargetMember = memberInfo;
                return sqlSegment;
            }

            if (memberExpr.TryGetParameters(out var parameterExprs))
            {
                if (parameterExprs.Count > 1)
                    throw new NotSupportedException($"不支持多参数访问，{memberExpr}");
                if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                    throw new NotSupportedException($"不支持多级成员访问，{memberExpr}");

                var parameterExpr = parameterExprs[0];
                var parameterName = parameterExpr.Name;
                var fromSegment = this.TableAliases[parameterName];

                if (fromSegment.Mapper != null)
                {
                    if (!fromSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        throw new NotSupportedException($"类{fromSegment.EntityType.FullName}没有成员{memberInfo.Name}，无法访问");
                    if (memberMapper.IsIgnore)
                        throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberInfo.Name}是忽略成员无法访问");
                    if (memberMapper.IsNavigation)
                        throw new NotSupportedException($"不支持导航属性成员访问，{memberExpr}");

                    sqlSegment.SqlType = SqlType.OnlyField;
                    sqlSegment.MemberMapper = memberMapper;
                    sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
                    sqlSegment.TypeHandler = memberMapper.TypeHandler;
                    sqlSegment.MemberName = memberMapper.FieldName;
                    var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                    if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                    sqlSegment.Value = fieldName;
                }
                //子查询和CTE子查询场景，fromSegment.TableType: TableType.FromQuery || TableType.CteSelfRef
                else
                {
                    var readerField = fromSegment.Fields.Find(f => f.TargetMember.Name == memberInfo.Name);
                    sqlSegment.SqlType = SqlType.OnlyField;
                    sqlSegment.MappedTargetType = readerField.MappedTargetType;
                    sqlSegment.TypeHandler = readerField.TypeHandler;
                    sqlSegment.MemberName = readerField.MemberName;
                    sqlSegment.Value = readerField.Value;
                }
                return sqlSegment;
            }
        }

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId));
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return sqlSegment.Change(ValueEvalutor.Evaluate(memberExpr), SqlType.Variable);
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
        var visitor = new HasParameterVisitor();
        visitor.Visit(newExpr);
        var sqlType = visitor.HasVariable ? SqlType.Variable : SqlType.Constant;
        return sqlSegment.Change(ValueEvalutor.Evaluate(newExpr), sqlType);
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
        this.WhereBuilder = null;
        this.TableAliasStart = 'a';
        this.IsNeedTableAlias = false;
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
    }
    public void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder builder)
    {
        var result = this.Visit(sqlSegment);
        if (builder.Length > 0)
            builder.Append(',');
        builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        if (result.IsNull) builder.Append("IS NULL");
        else builder.Append($"={this.WrapSql(sqlSegment)}");
    }
}