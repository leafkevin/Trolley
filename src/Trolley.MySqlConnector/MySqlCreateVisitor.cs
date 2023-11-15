using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlCreateVisitor : CreateVisitor, ICreateVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    protected StringBuilder UpdateFields { get; set; }
    protected bool IsUseUpdate { get; set; }
    protected bool IsUpdate { get; set; }

    public MySqlCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildCommand(IDbCommand command)
    {
        string sql = null;
        this.DbParameters = command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "WithBy":
                    this.VisitWithBy(deferredSegment.Value);
                    break;
                case "WithByField":
                    this.VisitWithByField((FieldObject)deferredSegment.Value);
                    break;
                case "WithBulk":
                    sql = this.BuildBulkSql(command);
                    break;
                case "SetObject":
                    this.UpdateFields = new();
                    this.VisitSetObject(deferredSegment.Value);
                    break;
                case "SetExpression":
                    this.UpdateFields = new();
                    this.VisitSet(deferredSegment.Value as LambdaExpression);
                    break;
            }
        }
        if (!this.IsBulk)
        {
            sql = this.BuildSql();
            command.CommandText = sql;
        }
        return sql;
    }
    public void OnDuplicateKeyUpdate(object updateObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetObject",
            Value = updateObj
        });
    }
    public void OnDuplicateKeyUpdate(Expression updateExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetExpression",
            Value = updateExpr
        });
    }
    public override string BuildHeadSql()
    {
        if (this.IsUseIgnoreInto) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public override string BuildTailSql()
    {
        if (this.UpdateFields != null && this.UpdateFields.Length > 0)
        {
            var sql = this.UpdateFields.Insert(0, " ON DUPLICATE KEY UPDATE ").ToString();
            this.UpdateFields.Clear();
            return sql;
        }
        if (this.Tables[0].Mapper.IsAutoIncrement && !this.IsBulk)
            return ";SELECT LAST_INSERT_ID()";
        return string.Empty;
    }
    public virtual void VisitSetObject(object updateObj)
    {
        var entityType = this.Tables[0].EntityType;
        var setFieldsInitializer = RepositoryHelper.BuildUpdateWithParameters(this, entityType, updateObj, false, this.IsMultiple);
        if (this.IsMultiple)
        {
            var typedSetFieldsInitializer = setFieldsInitializer as Action<IDataParameterCollection, StringBuilder, object, string>;
            typedSetFieldsInitializer.Invoke(this.DbParameters, this.UpdateFields, updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            //TODO:参数名称，已经被create子句使用
            var typedSetFieldsInitializer = setFieldsInitializer as Action<IDataParameterCollection, StringBuilder, object>;
            typedSetFieldsInitializer.Invoke(this.DbParameters, this.UpdateFields, updateObj);
        }
    }
    public virtual void VisitSet(LambdaExpression lambdaExpr)
    {
        this.IsUpdate = true;
        var currentExpr = lambdaExpr.Body;
        var entityType = this.Tables[0].EntityType;
        var callStack = new Stack<MethodCallExpression>();
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Parameter)
                break;

            if (currentExpr is MethodCallExpression callExpr)
            {
                callStack.Push(callExpr);
                currentExpr = callExpr.Object;
            }
        }
        var aliasName = "row";
        bool isAlias = false;
        this.InitTableAlias(lambdaExpr);
        while (callStack.TryPop(out var callExpr))
        {
            var genericArguments = callExpr.Method.GetGenericArguments();
            switch (callExpr.Method.Name)
            {
                case "Alias":
                    if (callExpr.Arguments.Count > 0)
                        aliasName = this.Evaluate<string>(callExpr.Arguments[0]);
                    isAlias = true;
                    break;
                case "Set":
                    //var genericType = genericArguments[0].DeclaringType;
                    if (callExpr.Arguments.Count == 1)
                    {
                        //Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[0].Type.BaseType == typeof(LambdaExpression))
                            this.VisitAndDeferred(new SqlSegment { Expression = callExpr.Arguments[0] });
                        //Set<TUpdateObj>(TUpdateObj updateObj)
                        else this.VisitSetObject(this.Evaluate(callExpr.Arguments[0]));
                    }
                    else if (callExpr.Arguments.Count == 2)
                    {
                        //Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[1].Type.BaseType == typeof(LambdaExpression))
                        {
                            var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                            if (condition) this.VisitAndDeferred(new SqlSegment { Expression = callExpr.Arguments[1] });
                        }
                        else
                        {
                            //Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition) this.VisitSetObject(this.Evaluate(callExpr.Arguments[1]));
                            }
                            //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
                            else this.VisitWithSetField(callExpr.Arguments[0], this.Evaluate(callExpr.Arguments[1]));
                        }
                    }
                    //Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
                    else
                    {
                        var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                        if (condition) this.VisitWithSetField(callExpr.Arguments[1], this.Evaluate(callExpr.Arguments[2]));
                    }
                    break;
                case "Values":
                    var memberExpr = callExpr.Arguments[0] as MemberExpression;
                    var memberMapper = this.Tables[0].Mapper.GetMemberMap(memberExpr.Member.Name);
                    var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                    if (isAlias) this.UpdateFields.Append($"{fieldName}=VALUES({aliasName}.{fieldName})");
                    else this.UpdateFields.Append($"{fieldName}=VALUES({fieldName})");
                    break;
            }
        }
        if (isAlias) this.UpdateFields.Append($" AS {aliasName}");
        this.IsUpdate = false;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            if (this.IsUpdate)
            {
                var entityMapper = this.Tables[0].Mapper;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    this.AddMemberElement(i, new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper }, memberMapper);
                }
                return sqlSegment;
            }
            else if (this.IsSelect)
            {
                var readerFields = new List<ReaderField>();
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], readerFields);
                }
                return sqlSegment.ChangeValue(readerFields);
            }
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        if (this.IsUpdate)
        {
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                    continue;

                this.AddMemberElement(i, new SqlSegment { Expression = memberAssignment.Expression, MemberMapper = memberMapper }, memberMapper);
            }
        }
        else if (this.IsSelect)
        {
            var readerFields = new List<ReaderField>();
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new Exception("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, readerFields);
            }
            return sqlSegment.ChangeValue(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    protected virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        foreach (var parameterExpr in parameters)
        {
            if (parameterExpr.Type == typeof(IMySqlCreateDuplicateKeyUpdate<>).MakeGenericType(this.Tables[0].EntityType))
                continue;
            this.TableAlias.TryAdd(parameterExpr.Name, this.Tables[0]);
        }
    }
    protected void AddMemberElement(int index, SqlSegment sqlSegment, MemberMap memberMapper)
    {
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        //只一个成员访问，没有设置语句，什么也不做，忽略
        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall
            && sqlSegment.FromMember.Name == memberMapper.MemberName)
            return;

        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        if (sqlSegment == SqlSegment.Null)
            this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=NULL");
        else if (sqlSegment.IsVariable || sqlSegment.IsConstant)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            this.AddDbParameter(memberMapper, parameterName, sqlSegment.Value);
            this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        else this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={sqlSegment.ToString()}");
    }
    protected virtual void VisitWithSetField(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        var addDbParametersDelegate = RepositoryHelper.BuildAddDbParameters(this.DbKey, this.OrmProvider, memberMapper, fieldValue);
        addDbParametersDelegate.Invoke(this.DbParameters, this.OrmProvider, parameterName, fieldValue);
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
}