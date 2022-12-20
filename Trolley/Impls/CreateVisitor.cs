using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class CreateVisitor : SqlVisitor
{
    private readonly IOrmDbFactory dbFactory;
    private readonly IOrmProvider ormProvider;
    private List<IDbDataParameter> dbParameters;
    private List<TableSegment> tables = new();
    private Dictionary<string, TableSegment> tableAlias = new();
    private string selectSql = null;
    private string whereSql = null;

    public CreateVisitor(IOrmDbFactory dbFactory, IOrmProvider ormProvider, Type entityType)
        : base(dbFactory, ormProvider)
    {
        this.dbFactory = dbFactory;
        this.ormProvider = ormProvider;
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.dbFactory.GetEntityMap(entityType)
        });
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.ormProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"INSERT INTO {entityTableName} {this.selectSql} FROM ");
        for (var i = 1; i < this.tables.Count; i++)
        {
            var tableSegment = this.tables[i];
            var tableName = tableSegment.Body;
            if (string.IsNullOrEmpty(tableName))
            {
                if (tableSegment.Mapper == null)
                    tableSegment.Mapper = this.dbFactory.GetEntityMap(tableSegment.EntityType);
                tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
            }
            if (i > 1) builder.Append(',');
            builder.Append($"{tableName} {tableSegment.AliasName}");
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public CreateVisitor From(Expression fieldSelector)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var sqlSegment = new SqlSegment { Expression = lambdaExpr.Body };
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                sqlSegment = this.VisitNew(sqlSegment);
                break;
            case ExpressionType.MemberInit:
                sqlSegment = this.VisitMemberInit(sqlSegment);
                break;
            default: throw new NotImplementedException("不支持的表达式，只支持New或MemberInit表达式，如: new { a.Id, b.Name + &quot;xxx&quot; } 或是new User { Id = a.Id, Name = b.Name + &quot;xxx&quot; }");
        }
        this.selectSql = sqlSegment.ToString();
        return this;
    }
    public CreateVisitor Where(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    protected override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        var insertBuilder = new StringBuilder("(");
        var fromBuilder = new StringBuilder(") SELECT ");
        var entityMapper = this.tables[0].Mapper;
        for (int i = 0; i < newExpr.Arguments.Count; i++)
        {
            var memberInfo = newExpr.Members[i];
            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out _))
                continue;
            this.AddMemberElement(i, sqlSegment.Next(newExpr.Arguments[i]), memberInfo, insertBuilder, fromBuilder);
        }
        insertBuilder.Append(fromBuilder);
        return sqlSegment.Change(insertBuilder.ToString());
    }
    protected override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var insertBuilder = new StringBuilder("(");
        var fromBuilder = new StringBuilder(") SELECT ");
        var entityMapper = this.tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out _))
                continue;
            this.AddMemberElement(i, sqlSegment.Next(memberAssignment.Expression), memberAssignment.Member, insertBuilder, fromBuilder);
        }
        insertBuilder.Append(fromBuilder);
        return sqlSegment.Change(insertBuilder.ToString());
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        char tableIndex = 'a';
        this.tableAlias.Clear();
        for (int i = 0; i < lambdaExpr.Parameters.Count - 1; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            var tableSegment = new TableSegment
            {
                EntityType = parameterExpr.Type,
                //可以省略
                //Mapper = this.dbFactory.GetEntityMap(parameterExpr.Type),
                AliasName = $"{(char)(tableIndex + i)}"
            };
            this.tables.Add(tableSegment);
            this.tableAlias.Add(parameterExpr.Name, tableSegment);
        }
    }
    private void AddMemberElement(int index, SqlSegment sqlSegment, MemberInfo memberInfo, StringBuilder insertBuilder, StringBuilder fromBuilder)
    {
        var nodeType = sqlSegment.Expression.NodeType;
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        var entityMapper = this.tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberInfo.Name);
        if (index > 0)
        {
            insertBuilder.Append(',');
            fromBuilder.Append(',');
        }
        insertBuilder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
        if (sqlSegment == SqlSegment.Null)
            fromBuilder.Append("NULL");
        else
        {
            if (nodeType == ExpressionType.Constant)
            {
                var parameterName = this.ormProvider.ParameterPrefix + memberInfo.Name;
                fromBuilder.Append(parameterName);
                this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, sqlSegment.Value));
            }
            else fromBuilder.Append(sqlSegment.ToString());
        }
    }
}
