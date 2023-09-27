using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    private List<UpdateDeferredSegment> deferredSegments = new();

    protected bool IsFrom { get; set; } = false;
    protected bool IsJoin { get; set; } = false;
    protected List<UpdateField> UpdateFields { get; set; } = new();
    protected bool HasFixedSet { get; set; }
    protected string FixedSql { get; set; }
    protected List<IDbDataParameter> FixedDbParameters { get; set; } = new();
    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public virtual void Initialize(Type entityType, bool isFirst = true)
    {
        if (isFirst)
        {
            this.Tables = new();
            this.TableAlias = new();
        }
        //clear
        else
        {
            this.IsFrom = false;
            this.IsJoin = false;
            this.deferredSegments.Clear();
            this.UpdateFields.Clear();
            this.FixedSql = null;
            this.FixedDbParameters.Clear();
            base.Clear();
        }
        this.Tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType)
        });
    }
    public string BuildCommand(IDbCommand command)
    {
        this.Command = command;
        int index = 0;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case DeferredUpdateType.Set:
                    this.VisitSet(deferredSegment.Value as Expression);
                    break;
                case DeferredUpdateType.SetField:
                    this.VisitSetField((FieldObject)deferredSegment.Value);
                    break;
                case DeferredUpdateType.SetWith:
                    this.VisitSetWith((FieldsParameters)deferredSegment.Value);
                    break;
                case DeferredUpdateType.SetFrom:
                    this.VisitSetFrom(deferredSegment.Value as Expression);
                    break;
                case DeferredUpdateType.SetFromField:
                    this.VisitSetFromField((FieldFromQuery)deferredSegment.Value);
                    break;
                case DeferredUpdateType.SetBulk:
                    var bulkBuilder = new StringBuilder();
                    this.SetBulkHead(bulkBuilder);
                    var updateObjs = deferredSegment.Value as IEnumerable;
                    foreach (var updateObj in updateObjs)
                    {
                        this.SetBulkMulti(bulkBuilder, updateObj, index);
                        index++;
                    }
                    return bulkBuilder.ToString();
                case DeferredUpdateType.Where:
                    this.VisitWhere(deferredSegment.Value as Expression);
                    break;
                case DeferredUpdateType.WhereWith:
                    this.VisitWhereWith(deferredSegment.Value);
                    break;
                case DeferredUpdateType.And:
                    this.VisitAnd(deferredSegment.Value as Expression);
                    break;
            }
        }
        return this.BuildSql();
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Update,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments
        };
    }
    public override int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<UpdateDeferredSegment>;
        int result = 1;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command));
        return result;
    }
    public virtual string BuildSql()
    {
        var entityTableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        var aliasName = this.Tables[0].AliasName;
        if (this.IsNeedAlias && aliasName.Length == 1)
            builder.Append($"{aliasName} ");

        if (this.IsJoin && this.Tables.Count > 1)
        {
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                builder.Append($" ON {tableSegment.OnExpr} ");
            }
        }
        int index = 0;
        bool hasWhere = false;
        builder.Append("SET ");
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type == UpdateFieldType.Where)
                {
                    hasWhere = true;
                    continue;
                }
                if (index > 0) builder.Append(',');
                switch (setField.Type)
                {
                    case UpdateFieldType.SetField:
                    case UpdateFieldType.SetValue:
                        if (this.IsNeedAlias) builder.Append($"{aliasName}.");
                        builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                        break;
                }
                index++;
            }
        }

        if (this.IsFrom && this.Tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql) || hasWhere)
            builder.Append(" WHERE ");
        if (hasWhere)
        {
            index = 0;
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type != UpdateFieldType.Where)
                    continue;
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                index++;
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            if (hasWhere)
                builder.Append(" AND ");
            builder.Append(this.WhereSql);
        }
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.IsFrom = true;
        int tableIndex = this.TableAsStart + this.Tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.Tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}"
            });
        }
        return this;
    }
    public virtual IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
    {
        this.IsNeedAlias = true;
        this.IsJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTable = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)('a' + this.Tables.Count)}",
            JoinType = joinType
        };
        this.Tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.Set,
            Value = fieldsAssignment
        });
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.SetField,
            Value = new FieldObject { FieldSelector = fieldSelector, FieldValue = fieldValue }
        });
        return this;
    }
    public virtual IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.SetWith,
            Value = new FieldsParameters { SelectorOrAssignment = fieldsSelectorOrAssignment, Parameters = updateObj }
        });
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.SetFrom,
            Value = fieldsAssignment
        });
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.SetFrom,
            Value = new FieldFromQuery { FieldSelector = fieldSelector, ValueSelector = valueSelector }
        });
        return this;
    }
    public virtual IUpdateVisitor SetBulkFirst(IDbCommand command, Expression fieldsSelectorOrAssignment, object updateObjs)
    {
        this.Command = command;
        var entityMapper = this.Tables[0].Mapper;
        var fixSetFields = new List<UpdateField>();
        var lambdaExpr = fieldsSelectorOrAssignment as LambdaExpression;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberExpr.Member.Name}";
                    this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                }
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberInfo.Name}";
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                    }
                    else this.AddMemberElement(sqlSegment, memberMapper, fixSetFields, this.FixedDbParameters);
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
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                    {
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
                    }
                    else this.AddMemberElement(sqlSegment, memberMapper, fixSetFields, this.FixedDbParameters);
                }
                break;
        }

        var fixedBuilder = new StringBuilder($"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ");
        if (fixSetFields.Count > 0)
        {
            for (int i = 0; i < fixSetFields.Count; i++)
            {
                var fixSetField = fixSetFields[i];
                if (i > 0) fixedBuilder.Append(',');
                fixedBuilder.Append($"{this.OrmProvider.GetFieldName(fixSetField.MemberMapper.FieldName)}={fixSetField.Value}");
            }
            this.HasFixedSet = true;
        }
        this.FixedSql = fixedBuilder.ToString();
        return this;
    }
    public virtual void SetBulkHead(StringBuilder builder)
    {
        if (this.FixedDbParameters != null && this.FixedDbParameters.Count > 0)
            this.FixedDbParameters.ForEach(f => this.Command.Parameters.Add(f));
    }
    public virtual void SetBulk(StringBuilder builder, object updateObj, int index)
    {
        builder.Append(this.FixedSql);
        if (this.HasFixedSet) builder.Append(',');
        int setIndex = 0;
        foreach (var setField in this.UpdateFields)
        {
            if (setIndex > 0) builder.Append(',');
            if (setField.Type == UpdateFieldType.SetValue)
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
            //SetField
            else
            {
                var fieldValue = this.EvaluateAndCache(updateObj, setField.MemberMapper.MemberName);
                var parameterName = $"{setField.Value}{index}";
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={parameterName}");
                this.Command.Parameters.Add(this.OrmProvider.CreateParameter(setField.MemberMapper, parameterName, fieldValue));
            }
            setIndex++;
        }
        builder.Append(" WHERE ");
        var keyMembers = this.Tables[0].Mapper.KeyMembers;
        for (int i = 0; i < keyMembers.Count; i++)
        {
            var keyMember = keyMembers[i];
            if (i > 0) builder.Append(" AND ");
            var parameterName = $"{this.OrmProvider.ParameterPrefix}k{keyMember.MemberName}{index}";
            builder.Append($"{this.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
            var fieldValue = this.EvaluateAndCache(updateObj, keyMember.MemberName);
            this.Command.Parameters.Add(this.OrmProvider.CreateParameter(keyMember, parameterName, fieldValue));
        }
    }
    protected virtual void SetBulkMulti(StringBuilder builder, object updateObj, int index)
    {
        builder.Append(this.FixedSql);
        if (this.HasFixedSet) builder.Append(',');
        int setIndex = 0;
        foreach (var setField in this.UpdateFields)
        {
            if (setIndex > 0) builder.Append(',');
            if (setField.Type == UpdateFieldType.SetValue)
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
            //SetField
            else
            {
                var fieldValue = this.EvaluateAndCache(updateObj, setField.MemberMapper.MemberName);
                var parameterName = $"{setField.Value}{index}_m{this.CommandIndex}";
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={parameterName}");
                this.Command.Parameters.Add(this.OrmProvider.CreateParameter(setField.MemberMapper, parameterName, fieldValue));
            }
            setIndex++;
        }
        builder.Append(" WHERE ");
        var keyMembers = this.Tables[0].Mapper.KeyMembers;
        for (int i = 0; i < keyMembers.Count; i++)
        {
            var keyMember = keyMembers[i];
            if (i > 0) builder.Append(" AND ");
            var parameterName = $"{this.OrmProvider.ParameterPrefix}k{keyMember.MemberName}{index}_m{this.CommandIndex}";
            builder.Append($"{this.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
            var fieldValue = this.EvaluateAndCache(updateObj, keyMember.MemberName);
            this.Command.Parameters.Add(this.OrmProvider.CreateParameter(keyMember, parameterName, fieldValue));
        }
    }
    public virtual void SetBulkTail(StringBuilder builder) { }
    public virtual IUpdateVisitor WhereWith(object whereObj)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.WhereWith,
            Value = whereObj
        });
        return this;
    }
    public virtual IUpdateVisitor Where(Expression whereExpr)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.Where,
            Value = whereExpr
        });
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
    {
        this.deferredSegments.Add(new UpdateDeferredSegment
        {
            Type = DeferredUpdateType.Where,
            Value = whereExpr
        });
        return this;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
            return formatter.Invoke(this, methodCallExpr, methodCallExpr.Object, sqlSegment.DeferredExprs, methodCallExpr.Arguments.ToArray());

        var lambdaExpr = Expression.Lambda(sqlSegment.Expression);
        var objValue = lambdaExpr.Compile().DynamicInvoke();
        if (objValue == null)
            return SqlSegment.Null;

        //把方法返回值当作常量处理
        return sqlSegment.Change(objValue, true, false, false);
    }
    protected void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAlias.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        int index = 0;
        foreach (var parameterExpr in lambdaExpr.Parameters)
        {
            if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
                continue;
            if (typeof(IFromQuery).IsAssignableFrom(parameterExpr.Type))
                continue;
            if (!parameters.Contains(parameterExpr.Name))
            {
                index++;
                continue;
            }
            this.TableAlias.Add(parameterExpr.Name, this.Tables[index]);
            index++;
        }
    }
    protected virtual void VisitSet(Expression fieldsAssignment)
    {
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
    protected virtual void VisitSetField(FieldObject fieldObject)
    {
        var lambdaExpr = fieldObject.FieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        this.AddMemberElement(memberMapper, fieldObject.FieldValue, false);
    }
    protected virtual void VisitSetWith(FieldsParameters fieldsParameters)
    {
        var entityMapper = this.Tables[0].Mapper;
        if (fieldsParameters.SelectorOrAssignment != null)
        {
            MemberMap memberMapper = null;
            var lambdaExpr = fieldsParameters.SelectorOrAssignment as LambdaExpression;
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    this.AddMemberElement(memberMapper, fieldsParameters.Parameters);
                    break;
                case ExpressionType.New:
                    this.InitTableAlias(lambdaExpr);
                    var newExpr = lambdaExpr.Body as NewExpression;
                    for (int i = 0; i < newExpr.Arguments.Count; i++)
                    {
                        var memberInfo = newExpr.Members[i];
                        if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                            continue;

                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            this.AddMemberElement(memberMapper, fieldsParameters.Parameters);
                        else this.AddMemberElement(sqlSegment, memberMapper);
                    }
                    break;
                case ExpressionType.MemberInit:
                    this.InitTableAlias(lambdaExpr);
                    var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                    for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                    {
                        var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                        if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out memberMapper))
                            continue;

                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                            this.AddMemberElement(memberMapper, fieldsParameters.Parameters);
                        else this.AddMemberElement(sqlSegment, memberMapper);
                    }
                    break;
            }
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildUpdateWithParameters(this, entityMapper.EntityType, fieldsParameters.Parameters, false, this.IsMultiple);
            if (this.IsMultiple)
            {
                var multiCommandInitializer = commandInitializer as Action<IDbCommand, List<UpdateField>, object, int>;
                multiCommandInitializer.Invoke(this.Command, this.UpdateFields, fieldsParameters.Parameters, this.CommandIndex);
            }
            else
            {
                var singleCommandInitializer = commandInitializer as Action<IDbCommand, List<UpdateField>, object>;
                singleCommandInitializer.Invoke(this.Command, this.UpdateFields, fieldsParameters.Parameters);
            }
        }
    }
    protected virtual void VisitSetFrom(Expression fieldsAssignment)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
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

                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr.GetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                            continue;
                        this.AddMemberElement(sqlSegment, memberMapper);
                    }
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

                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr.GetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                            continue;
                        this.AddMemberElement(sqlSegment, memberMapper);
                    }
                }
                break;
        }
    }
    protected virtual void VisitSetFromField(FieldFromQuery fieldFrom)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldFrom.FieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        this.InitTableAlias(fieldFrom.ValueSelector as LambdaExpression);
        var sql = this.VisitFromQuery(fieldFrom.ValueSelector as LambdaExpression, out var isNeedAlias);
        if (isNeedAlias) this.IsNeedAlias = true;
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = $"({sql})" });
    }
    protected virtual void VisitWhereWith(object whereObj)
    {
        var entityMapper = this.Tables[0].Mapper;
        var commandInitializer = RepositoryHelper.BuildUpdateWithParameters(this, entityMapper.EntityType, whereObj, true, this.IsMultiple);
        if (this.IsMultiple)
        {
            var multiCommandInitializer = commandInitializer as Action<IDbCommand, List<UpdateField>, object, int>;
            multiCommandInitializer.Invoke(this.Command, this.UpdateFields, whereObj, this.CommandIndex);
        }
        else
        {
            var singleCommandInitializer = commandInitializer as Action<IDbCommand, List<UpdateField>, object>;
            singleCommandInitializer.Invoke(this.Command, this.UpdateFields, whereObj);
        }
    }
    protected virtual void VisitWhere(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND ";
        this.WhereSql += whereSql;
        this.IsWhere = false;
    }
    protected virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
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
    protected void AddMemberElement(MemberMap memberMapper, object memberValue, bool isEntity = true)
    {
        if (memberValue is DBNull || memberValue == null)
        {
            this.UpdateFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        var fieldValue = isEntity ? this.EvaluateAndCache(memberValue, memberMapper.MemberName) : memberValue;
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        this.Command.Parameters.Add(this.OrmProvider.CreateParameter(memberMapper, parameterName, fieldValue));
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = parameterName });
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
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper, List<UpdateField> setFields, List<IDbDataParameter> dbParameters)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            setFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }

        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + dbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            IDbDataParameter dbParameter = null;
            if (memberMapper != null)
                dbParameter = this.OrmProvider.CreateParameter(memberMapper, parameterName, sqlSegment.Value);
            else dbParameter = this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value);

            dbParameters.Add(dbParameter);
            sqlSegment.Value = parameterName;
            //sqlSegment.IsParameter = true;
            //sqlSegment.IsVariable = false;
            //sqlSegment.IsConstant = false;
        }
        setFields.Add(new UpdateField { MemberMapper = memberMapper, Value = sqlSegment.Value.ToString() });
    }
    enum DeferredUpdateType
    {
        Set,
        SetWith,
        SetField,
        SetFrom,
        SetFromField,
        SetBulk,
        WhereWith,
        Where,
        And
    }
    struct UpdateDeferredSegment
    {
        public DeferredUpdateType Type { get; set; }
        public object Value { get; set; }
    }
    struct UpdateDeferredCommand
    {
        public Type EntityType { get; set; }
        public List<UpdateDeferredSegment> Segments { get; set; }
    }
}
