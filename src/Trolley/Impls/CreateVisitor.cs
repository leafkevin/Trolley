using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private string bulkHeadSql;
    private object bulkCommandInitializer;
    private List<InsertDeferredSegment> deferredSegments = new();

    protected List<InsertField> InsertFields { get; set; } = new();
    protected List<UpdateField> UpdateFields { get; set; } = new();
    protected bool IsUseIgnore { get; set; }
    protected bool IsUseIfNotExists { get; set; }
    protected bool IsUseOrUpdate { get; set; }
    protected virtual bool IsBulk { get; set; } = false;

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public virtual void Initialize(Type entityType, bool isFirst = true)
    {
        if (isFirst)
        {
            this.Tables = new()
            {
                new TableSegment
                {
                    EntityType = entityType,
                    Mapper = this.MapProvider.GetEntityMap(entityType)
                }
            };
            this.TableAlias = new();
        }
        //clear
        else
        {
            this.deferredSegments.Clear();
            this.InsertFields.Clear();
            this.InsertFields.Clear();
            this.IsUseIgnore = false;
            this.IsUseIfNotExists = false;
            this.IsUseOrUpdate = false;
            this.IsBulk = false;
            this.bulkCommandInitializer = null;
            base.Clear();
        }
    }
    public string BuildCommand(IDbCommand command)
    {
        this.Command = command;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case DeferredInsertType.WithBy:
                    this.VisitWithBy(deferredSegment.Value);
                    break;
                case DeferredInsertType.WithByField:
                    this.VisitWithByField((FieldObject)deferredSegment.Value);
                    break;
                case DeferredInsertType.WithBulk:
                    int index = 0;
                    this.IsBulk = true;
                    var builder = new StringBuilder();
                    var bulkObject = (BulkObject)this.deferredSegments[0].Value;
                    foreach (var entity in bulkObject.BulkObjects)
                    {
                        this.WithBulk(builder, entity, index);
                        index++;
                    }
                    return builder.ToString();
                    //case DeferredInsertType.SetObject:
                    //    this.VisitSet(command, deferredSegment.Value, this.CommandIndex);
                    //    break;
                    //case DeferredInsertType.SetExpression:
                    //    this.VisitSet(command, deferredSegment.Value as Expression, this.CommandIndex);
                    //    break;
            }
        }

        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
        var valuesBuilder = new StringBuilder();
        if (this.IsUseIfNotExists) valuesBuilder.Append(" SELECT ");
        else valuesBuilder.Append(" VALUES(");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(insertField.Fields);
            valuesBuilder.Append(insertField.Values);
        }
        fieldsBuilder.Append(')');
        if (this.IsUseIfNotExists)
        {
            if (!string.IsNullOrEmpty(this.WhereSql))
                valuesBuilder.Append(" WHERE " + this.WhereSql);
        }
        else valuesBuilder.Append(')');

        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            valuesBuilder.Append(tailSql);
        fieldsBuilder.Append(valuesBuilder);
        return fieldsBuilder.ToString();
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        var multiCommand = new MultipleCommand
        {
            CommandType = MultipleCommandType.Insert,
            Body = new InsertDeferredCommand
            {
                EntityType = this.Tables[0].EntityType,
                Segments = this.deferredSegments
            }
        };
        return multiCommand;
    }
    public override int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        var deferredCommand = (InsertDeferredCommand)multiCommand.Body;
        this.deferredSegments = deferredCommand.Segments;
        int result = 1;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command));
        return result;
    }
    public virtual string BuildHeadSql() => $"INSERT INTO";
    public virtual string BuildTailSql() => string.Empty;
    public virtual ICreateVisitor UseIgnore()
    {
        this.IsUseIgnore = true;
        return this;
    }
    public virtual ICreateVisitor IfNotExists(object whereObj)
    {
        this.IsUseIfNotExists = true;
        //var commandInitializer = RepositoryHelper.BuildWhereWithKeysSqlParameters(this, this.Tables[0].EntityType, whereObj);
        //this.WhereSql = commandInitializer.Invoke(this, this.DbParameters, whereObj);
        return this;
    }
    public virtual ICreateVisitor IfNotExists(Expression keysPredicate)
    {
        this.IsUseIfNotExists = true;
        this.IsWhere = true;
        var lambdaExpr = keysPredicate as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
        return this;
    }
    //public virtual ICreateVisitor Set(Expression fieldsAssignment, int commandIndex)
    //{
    //    this.IsUseOrUpdate = true;
    //    var lambdaExpr = fieldsAssignment as LambdaExpression;
    //    var entityMapper = this.Tables[0].Mapper;
    //    switch (lambdaExpr.Body.NodeType)
    //    {
    //        case ExpressionType.New:
    //            this.InitTableAlias(lambdaExpr);
    //            var newExpr = lambdaExpr.Body as NewExpression;
    //            for (int i = 0; i < newExpr.Arguments.Count; i++)
    //            {
    //                var memberInfo = newExpr.Members[i];
    //                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
    //                    continue;

    //                var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
    //                //只一个成员访问，没有设置语句，什么也不做，忽略
    //                if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
    //                    continue;
    //                this.AddMemberElement(sqlSegment, memberMapper, commandIndex);
    //            }
    //            break;
    //        case ExpressionType.MemberInit:
    //            this.InitTableAlias(lambdaExpr);
    //            var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
    //            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
    //            {
    //                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
    //                if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
    //                    continue;

    //                var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
    //                //只一个成员访问，没有设置语句，什么也不做，忽略
    //                if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
    //                    continue;
    //                this.AddMemberElement(sqlSegment, memberMapper, commandIndex);
    //            }
    //            break;
    //    }
    //    return this;
    //}
    //public virtual ICreateVisitor Set(object updateObj, int commandIndex)
    //{
    //    this.IsUseOrUpdate = true;
    //    var entityMapper = this.Tables[0].Mapper;
    //    var parametersInitializer = RepositoryHelper.BuildUpdateSetWithParameters(this, entityMapper.EntityType, updateObj, this.IsMulti, commandIndex);
    //    parametersInitializer.Invoke(this, this.UpdateFields, this.DbParameters, updateObj);
    //    return this;
    //}
    public virtual ICreateVisitor WithBy(object insertObj)
    {
        this.deferredSegments.Add(new InsertDeferredSegment
        {
            Type = DeferredInsertType.WithBy,
            Value = insertObj
        });
        return this;
    }
    public virtual ICreateVisitor WithByField(FieldObject fieldObject)
    {
        this.deferredSegments.Add(new InsertDeferredSegment
        {
            Type = DeferredInsertType.WithByField,
            Value = fieldObject
        });
        return this;
    }
    public virtual ICreateVisitor WithBulkFirst(IDbCommand command, IEnumerable insertObjs)
    {
        this.IsBulk = true;
        this.Command = command;
        var entityTppe = this.Tables[0].EntityType;
        var commandInitializer = RepositoryHelper.BuildCreateWithBulkCommandInitializer(this, entityTppe, insertObjs, this.IsMultiple, out var bulkHeadSql);
        if (this.IsMultiple)
        {
            this.deferredSegments.Add(new InsertDeferredSegment
            {
                Type = DeferredInsertType.WithBulk,
                Value = new BulkObject
                {
                    HeadSql = bulkHeadSql,
                    CommandInitializer = commandInitializer,
                    BulkObjects = insertObjs
                }
            });
        }
        else
        {
            this.bulkHeadSql = bulkHeadSql;
            this.bulkCommandInitializer = commandInitializer;
        }
        return this;
    }
    public virtual ICreateVisitor WithBulk(StringBuilder builder, object insertObj, int index)
    {
        if (index == 0)
        {
            var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
            builder.Append($"{this.BuildHeadSql()} {tableName} ({this.bulkHeadSql}) VALUES ");
        }
        else builder.Append(',');
        builder.Append('(');
        if (this.IsMultiple)
        {
            var multiCommandInitializer = this.bulkCommandInitializer as Action<IDbCommand, object, StringBuilder, int, int>;
            multiCommandInitializer.Invoke(this.Command, insertObj, builder, index, this.CommandIndex);
        }
        else
        {
            var singleCommandInitializer = this.bulkCommandInitializer as Action<IDbCommand, object, StringBuilder, int>;
            singleCommandInitializer.Invoke(this.Command, insertObj, builder, index);
        }
        builder.Append(')');
        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            builder.Append(tailSql);
        return this;
    }
    public virtual IQueryVisitor CreateQuery(params Type[] sourceTypes)
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix);
        queryVisiter.From(this.TableAsStart, sourceTypes);
        return queryVisiter;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var fieldsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;

                this.AddMemberElement(i, new SqlSegment { Expression = newExpr.Arguments[i] }, memberMapper, fieldsBuilder, valuesBuilder);
            }
            return sqlSegment.ChangeValue(new InsertField
            {
                Fields = fieldsBuilder.ToString(),
                Values = valuesBuilder.ToString()
            });
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            this.AddMemberElement(i, new SqlSegment { Expression = memberAssignment.Expression }, memberMapper, fieldsBuilder, valuesBuilder);
        }
        return sqlSegment.ChangeValue(new InsertField
        {
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }

    protected virtual void VisitWithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var commandInitializer = RepositoryHelper.BuildCreateWithBiesCommandInitializer(this, entityType, insertObj, this.IsMultiple);
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        if (this.IsMultiple)
        {
            var multiCommandInitializer = commandInitializer as Action<IDbCommand, object, StringBuilder, StringBuilder, int>;
            multiCommandInitializer.Invoke(this.Command, insertObj, fieldsBuilder, valuesBuilder, this.CommandIndex);
        }
        else
        {
            var singleCommandInitializer = commandInitializer as Action<IDbCommand, object, StringBuilder, StringBuilder>;
            singleCommandInitializer.Invoke(this.Command, insertObj, fieldsBuilder, valuesBuilder);
        }
        this.InsertFields.Add(new InsertField
        {
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }
    protected virtual void VisitWithByField(FieldObject fieldObject)
    {
        var lambdaExpr = fieldObject.Selector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        this.Command.Parameters.Add(this.OrmProvider.CreateParameter(memberMapper, parameterName, fieldObject.Value));
        this.InsertFields.Add(new InsertField
        {
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = parameterName
        });
    }
    protected virtual void VisitSet(Expression fieldsAssignment)
    {
        this.IsUseOrUpdate = true;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
        }
    }
    protected virtual void VisitSet(object updateObj)
    {
        this.IsUseOrUpdate = true;
        var entityMapper = this.Tables[0].Mapper;
        //var parametersInitializer = RepositoryHelper.BuildUpdateSetWithParameters(this, entityMapper.EntityType, updateObj);
        //parametersInitializer.Invoke(this, this.UpdateFields, this.DbParameters, updateObj);
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            this.TableAlias.Add(parameterExpr.Name, this.Tables[i + 1]);
        }
    }
    protected void AddMemberElement(int index, SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder fieldsBuilder, StringBuilder valuesBuilder)
    {
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        if (index > 0)
        {
            fieldsBuilder.Append(',');
            valuesBuilder.Append(',');
        }
        fieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        if (sqlSegment == SqlSegment.Null)
            valuesBuilder.Append("NULL");
        else
        {
            sqlSegment.IsParameterized = true;
            sqlSegment.MemberMapper = memberMapper;
            sqlSegment.ParameterName = memberMapper.MemberName;
            if (this.IsMultiple) sqlSegment.ParameterName += $"_m{this.CommandIndex}";
            valuesBuilder.Append(this.GetQuotedValue(sqlSegment));
        }
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.UpdateFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        sqlSegment.IsParameterized = true;
        sqlSegment.MemberMapper = memberMapper;
        sqlSegment.ParameterName = memberMapper.MemberName;
        if (this.IsMultiple) sqlSegment.ParameterName += $"_m{this.CommandIndex}";
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
    }
}
public struct InsertField
{
    public string Fields { get; set; }
    public string Values { get; set; }
}