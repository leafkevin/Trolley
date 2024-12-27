using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlCreateVisitor : CreateVisitor
{
    public StringBuilder UpdateBuilder { get; set; }
    public bool IsUpdate { get; set; }
    /// <summary>
    /// 当有OnConflict更新操作时，引用原值时才会设置，使用IsNeedTableAlias会影响正常Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)场景的解析
    /// </summary>
    public bool IsUseTableAlias { get; set; }
    public string FromSql { get; set; }
    public string OutputSql { get; set; }

    public PostgreSqlCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildWithBulkSql(command, out readerFields);
        else
        {
            //多命令执行时，第二次以后DbParameters有值，并且就是command.Parameters
            //当Insert Select From操作时，DbParameters也有值，但不是command.Parameters，需要赋值到command.Parameters
            if (this.DbParameters != null && this.DbParameters != command.Parameters)
            {
                foreach (var dbParameter in this.DbParameters)
                {
                    command.Parameters.Add(dbParameter);
                }
                this.DbParameters = command.Parameters;
            }
            else this.DbParameters ??= command.Parameters;

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
                    case "SetExpression":
                        this.VisitSetExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public override string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = this.ReaderFields;
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var entityMapper = tableSegment.Mapper;

        if (string.IsNullOrEmpty(this.FromSql))
        {
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
            //Set语句中，引用了原值，就需要使用别名
            if (this.IsUseTableAlias) tableName += $" AS {tableSegment.AliasName}";

            if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
                throw new NotSupportedException("返回Identity，不支持同时Returning操作");
            this.FromSql = $"INSERT INTO {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
        }

        string tailSql = string.Empty;
        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();

        if (this.OutputSql != null)
            tailSql += this.OutputSql;

        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
            tailSql = this.OrmProvider.GetIdentitySql(keyFieldName);
        }
        return $"{this.FromSql}{tailSql}";
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
                    case "SetExpression":
                        this.VisitSetExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields/OnConflict/Returning操作");
                }
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            }
            fixedSql = $"({this.ValuesBuilder}";
        }

        var entityMapper = tableSegment.Mapper;
        var fieldsSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.DbContext, entityType, insertObjType, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSetter = RepositoryHelper.BuildCreateValuesSqlPart(this.DbContext, entityType, insertObjType, true, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

        string headSql = "INSERT INTO ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql = $"INSERT INTO {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.";

        //生成批量Fields SQL
        fieldsSetter.Invoke(this.FieldsBuilder, this.DbContext, firstInsertObj);

        string tailSql = string.Empty;
        List<SqlFieldSegment> readerFields = null;

        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();
        if (this.OutputSql != null)
        {
            tailSql += this.OutputSql;
            readerFields = this.ReaderFields;
        }

        var fieldsSql = $" ({this.FieldsBuilder}) VALUES ";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)}{fieldsSql}");
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else firstSqlSetter = (dbParameters, builder, tableName) => builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)}{fieldsSql}");

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
    public void Returning(string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = fieldNames
        });
    }
    public virtual void Returning(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
    public void WithBulkCopy(IEnumerable insertObjs)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = insertObjs
        });
    }
    public IEnumerable BuildWithBulkCopy() => (IEnumerable)this.deferredSegments[0].Value;
    public void OnConflict(Expression updateExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetExpression",
            Value = updateExpr
        });
    }
    public void VisitSetObject(object updateObj)
    {
        var entityType = this.Tables[0].EntityType;
        var updateObjType = updateObj.GetType();
        var setFieldsSetter = RepositoryHelper.BuildFieldsSqlParametersPart(this.DbContext, entityType, updateObjType, 3, 1, 0, false, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        if (this.IsMultiple)
        {
            var typedSetFieldsSetter = setFieldsSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            typedSetFieldsSetter.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedSetFieldsSetter = setFieldsSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedSetFieldsSetter.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj);
        }
    }
    public void VisitSetExpression(LambdaExpression lambdaExpr)
    {
        var currentExpr = lambdaExpr.Body;
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
        this.InitTableAlias(lambdaExpr);
        this.UpdateBuilder ??= new();
        var builder = new StringBuilder(" ON CONFLICT ");
        while (callStack.TryPop(out var callExpr))
        {
            switch (callExpr.Method.Name)
            {
                case "DoNothing":
                    builder.Append("DO NOTHING");
                    break;
                case "UseKeys":
                    builder.Append('(');
                    foreach (var keyMapper in this.Tables[0].Mapper.KeyMembers)
                    {
                        builder.Append(this.OrmProvider.GetFieldName(keyMapper.FieldName));
                    }
                    builder.Append(") DO UPDATE SET ");
                    break;
                case "UseConstraint":
                    var constraintName = this.Evaluate<string>(callExpr.Arguments[0]);
                    if (string.IsNullOrEmpty(constraintName))
                        throw new ArgumentNullException("参数constraintName不能为null");
                    builder.Append($" {constraintName} DO UPDATE SET ");
                    break;
                case "Set":
                    //var genericType = genericArguments[0].DeclaringType;
                    if (callExpr.Arguments.Count == 1)
                    {
                        this.IsUpdate = true;
                        //更新时的成员访问就是引用原值
                        //Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[0].Type.BaseType == typeof(LambdaExpression))
                        {
                            this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[0] });
                        }
                        //Set<TUpdateObj>(TUpdateObj updateObj), 走参数
                        else this.VisitSetObject(this.Evaluate(callExpr.Arguments[0]));
                        this.IsUpdate = false;
                    }
                    else if (callExpr.Arguments.Count == 2)
                    {
                        //Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
                        //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                        if (callExpr.Arguments[1].Type.BaseType == typeof(LambdaExpression))
                        {
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition)
                                {
                                    this.IsUpdate = true;
                                    //更新时的成员访问就是引用原值
                                    this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[1] });
                                    this.IsUpdate = false;
                                }
                            }
                            else this.VisitSetFieldExpression(callExpr.Arguments[0], callExpr.Arguments[1]);
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
                    //Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                    else
                    {
                        var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                        if (condition)
                        {
                            if (callExpr.Arguments[2].Type.BaseType == typeof(LambdaExpression))
                                this.VisitSetFieldExpression(callExpr.Arguments[1], callExpr.Arguments[2]);
                            else this.VisitWithSetField(callExpr.Arguments[1], this.Evaluate(callExpr.Arguments[2]));
                        }
                    }
                    break;
                case "Where":
                    this.IsUpdate = true;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[0] });
                    this.UpdateBuilder.Append($" WHERE {sqlSegment.Body}");
                    this.IsUpdate = false;
                    break;
            }
        }
        this.UpdateBuilder.Insert(0, builder.ToString());
        builder.Clear();
    }
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        if (this.IsUpdate)
        {
            var memberExpr = sqlSegment.Expression as MemberExpression;
            if (!this.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                throw new MissingMemberException($"类{this.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

            //更新时的成员访问就是引用原值
            this.IsUseTableAlias = true;
            var fieldName = $"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}";

            sqlSegment.HasField = true;
            sqlSegment.SegmentType = memberMapper.MemberType;
            sqlSegment.FromMember = memberMapper.Member;
            sqlSegment.NativeDbType = memberMapper.NativeDbType;
            sqlSegment.TypeHandler = memberMapper.TypeHandler;
            sqlSegment.Body = fieldName;
            return sqlSegment;
        }
        return base.VisitMemberAccess(sqlSegment);
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        //只有OnDuplicateKeyUpdate.Set时，才会走到此场景，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
        //INSERT INTO ... SELECT ... FROM ... 由FromCommand单独处理了，FromCommand走的是QueryVisitor的解析
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;
                sqlSegment = this.VisitAndDeferred(new SqlFieldSegment
                {
                    Expression = newExpr.Arguments[i],
                    NativeDbType = memberMapper.NativeDbType,
                    TypeHandler = memberMapper.TypeHandler
                });
                this.AddMemberElement(sqlSegment, memberMapper);
            }
            return sqlSegment;
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var entityMapper = this.Tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            sqlSegment = this.VisitAndDeferred(new SqlFieldSegment
            {
                Expression = memberAssignment.Expression,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler
            });
            this.AddMemberElement(sqlSegment, memberMapper);
        }
        return this.Evaluate(sqlSegment);
    }
    public override IQueryVisitor CreateQueryVisitor()
    {
        var queryVisiter = new PostgreSqlQueryVisitor(this.DbContext, this.TableAsStart, this.DbParameters)
        {
            IsMultiple = this.IsMultiple,
            CommandIndex = this.CommandIndex,
            RefQueries = this.RefQueries,
            ShardingTables = this.ShardingTables
        };
        return queryVisiter;
    }
    public void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        foreach (var parameterExpr in parameters)
        {
            if (parameterExpr.Type == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(this.Tables[0].EntityType))
                continue;
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[0]);
        }
    }
    public void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper)
    {
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);

        if (sqlSegment == SqlFieldSegment.Null)
            this.UpdateBuilder.Append($"{fieldName}=NULL");
        else if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            var dbFieldValue = sqlSegment.Value;
            if (memberMapper.TypeHandler != null)
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, dbFieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(dbFieldValue.GetType(), targetType, false, this.Options);
                dbFieldValue = valueGetter.Invoke(dbFieldValue);
            }

            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            this.UpdateBuilder.Append($"{fieldName}={parameterName}");
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //.Set(true, f => f.TotalAmount))
        //.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        else this.UpdateBuilder.Append($"{fieldName}={sqlSegment.Body}");
    }
    public void VisitSetFieldExpression(Expression fieldSelector, Expression fieldValueSelector)
    {
        var fieldSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldSelector });
        this.IsUpdate = true;
        var valueSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldValueSelector });
        this.IsUpdate = false;
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{fieldSegment.Body}={valueSegment.Body}");
    }
    public void VisitWithSetField(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = this.EnsureLambda(fieldSelector);
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        //在前面insert的时候，参数有可能已经添加过了，此处需要判断是否需要添加
        if (!this.DbParameters.Contains(parameterName))
        {
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.Options);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
        }
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    public void VisitOutputFields(string fieldNames)
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
                this.ReaderFields.Add(new SqlFieldSegment
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
            this.ReaderFields.Add(new SqlFieldSegment
            {
                FieldType = SqlFieldType.RawSql,
                Body = fieldNames
            });
        }
    }
    public void VisitOutputExpression(LambdaExpression fieldsSelector)
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
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberExpr.Member;
                    sqlSegment.SegmentType = memberExpr.Type;
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall
                        || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != sqlSegment.TargetMember.Name)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberAssignment.Member;
                    sqlSegment.SegmentType = memberAssignment.Member.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.OutputSql = null;
    }
}