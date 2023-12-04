using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private static ConcurrentDictionary<int, Action<StringBuilder, object>> headSqlSetterCache = new();
    private static ConcurrentDictionary<int, object> valuesSqlParametersCache = new();
    private static ConcurrentDictionary<int, object> multiValuesSqlParametersCache = new();

    protected List<CommandSegment> deferredSegments = new();

    public string[] OnlyFieldNames { get; set; }
    public string[] IgnoreFieldNames { get; set; }
    public List<FieldsSegment> InsertFields { get; set; } = new();
    public bool IsBulk { get; set; }
    public virtual bool IsFromWith { get; set; }
    public virtual bool IsReturnIdentity { get; set; }

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public virtual void Initialize(Type entityType, bool isFirst = true)
    {
        if (isFirst)
        {
            this.Tables = new();
            this.TableAlias = new();
        }
        //clear
        else this.Clear();
        this.Tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType)
        });
    }
    public virtual string BuildCommand(IDbCommand command, bool isReturnIdentity)
    {
        string sql = null;
        this.IsReturnIdentity = isReturnIdentity;
        this.DbParameters = command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "WithBy":
                    this.VisitWithBy(deferredSegment.Value);
                    break;
                case "WithByField":
                    (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                    this.VisitWithByField(fieldSelector, fieldValue);
                    break;
                case "WithBulk":
                    //这里只处理多语句执行，分批次在最外层单独处理
                    if (this.IsMultiple)
                        sql = this.BuildMultiBulkSql(command);
                    break;
                case "FromWith":
                    (Delegate cteSubQuery, string cteTableName) = ((Delegate, string))deferredSegment.Value;
                    sql = this.VisitFromWith(cteSubQuery, cteTableName);
                    break;
            }
        }
        if (sql == null) sql = this.BuildSql();
        command.CommandText = sql;
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Insert,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments
        };
    }
    public void BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command, false));
    }
    public virtual string BuildSql()
    {
        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"INSERT INTO {tableName} (");
        var valuesBuilder = new StringBuilder(" VALUES(");
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
        valuesBuilder.Append(')');

        if (this.IsReturnIdentity)
        {
            if (!this.Tables[0].Mapper.IsAutoIncrement)
                throw new Exception($"实体{this.Tables[0].Mapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            valuesBuilder.Append(this.OrmProvider.GetIdentitySql(this.Tables[0].EntityType));
        }
        fieldsBuilder.Append(valuesBuilder);
        valuesBuilder.Clear();
        return fieldsBuilder.ToString();
    }
    public virtual ICreateVisitor WithBy(object insertObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBy",
            Value = insertObj
        });
        return this;
    }
    public virtual ICreateVisitor WithByField(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByField",
            Value = (fieldSelector, fieldValue)
        });
        return this;
    }
    public virtual ICreateVisitor WithBulk(object insertObjs, int bulkCount)
    {
        this.IsBulk = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulk",
            Value = (insertObjs as IEnumerable, bulkCount)
        });
        return this;
    }
    public virtual ICreateVisitor WithFrom<TTarget>(Func<IFromQuery, IQuery<TTarget>> cteSubQuery, string cteTableName = null)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "FromWith",
            Value = (cteSubQuery, cteTableName)
        });
        return this;
    }
    public virtual ICreateVisitor IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames = fieldNames;
        return this;
    }
    public virtual ICreateVisitor IgnoreFields(Expression fieldsSelector)
    {
        this.IgnoreFieldNames = this.VisitFields(fieldsSelector);
        return this;
    }
    public virtual ICreateVisitor OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames = fieldNames;
        return this;
    }
    public virtual ICreateVisitor OnlyFields(Expression fieldsSelector)
    {
        this.OnlyFieldNames = this.VisitFields(fieldsSelector);
        return this;
    }
    public virtual string BuildMultiBulkSql(IDbCommand command)
    {
        if (!this.IsMultiple) return null;

        int index = 0;
        var builder = new StringBuilder();
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        var entityType = this.Tables[0].EntityType;
        //多语句执行，一次性不分批次
        (var headSqlSetter, var commandInitializer) = RepositoryHelper.BuildCreateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, this.OnlyFieldNames, this.IgnoreFieldNames, false);
        foreach (var insertObj in insertObjs)
        {
            if (index > 0) builder.Append(',');
            commandInitializer.Invoke(command.Parameters, this.OrmProvider, builder, insertObj, $"_m{this.CommandIndex}");
            index++;
        }
        return builder.ToString();
    }
    public virtual (IEnumerable, int, Action<StringBuilder>, Action<StringBuilder, object, string>) BuildWithBulk(IDbCommand command)
    {
        this.IsBulk = true;
        this.DbParameters = command.Parameters;
        var entityType = this.Tables[0].EntityType;
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        if (this.deferredSegments.Count > 1)
        {
            object insertObj = null;
            foreach (var entity in insertObjs)
            {
                insertObj = entity;
                break;
            }
            var insertObjType = insertObjs.GetType();
            var entityMapper = this.Tables[0].Mapper;

            var cacheKey = HashCode.Combine(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
            var headSqlBulkSetter = headSqlSetterCache.GetOrAdd(cacheKey, f => RepositoryHelper.BuildCreateHeadSqlPart(
                this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames));

            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                        this.VisitWithByField(fieldSelector, fieldValue);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields操作");
                }
            }
            var fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            Action<StringBuilder> headSqlSetter = (builder) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(entityMapper.TableName)} (");
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Fields);
                }
                headSqlBulkSetter.Invoke(builder, insertObj);
                builder.Append(") VALUES ");
                if (fixedDbParameters.Count > 0)
                    fixedDbParameters.ForEach(f => this.DbParameters.Add(f));
            };
            var valuesPartSqlParameters = multiValuesSqlParametersCache.GetOrAdd(cacheKey, f => RepositoryHelper.BuildCreateValuesPartSqlParametes(
                this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true));
            var typedValuesPartSqlParameters = valuesPartSqlParameters as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, string>;

            this.DbParameters.Clear();
            Action<StringBuilder, object, string> commandInitializer = null;
            commandInitializer = (builder, insertObj, suffix) =>
            {
                builder.Append('(');
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Values);
                }
                fixedDbParameters.ForEach(f => this.DbParameters.Add(f));
                typedValuesPartSqlParameters.Invoke(this.DbParameters, this.OrmProvider, builder, insertObj, suffix);
                builder.Append(')');
            };
            return (insertObjs, bulkCount, headSqlSetter, commandInitializer);
        }
        else
        {
            (var headSqlSetter, var typedCommandInitializer) = RepositoryHelper.BuildCreateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, this.OnlyFieldNames, this.IgnoreFieldNames, this.IsMultiple);
            Action<StringBuilder, object, string> commandInitializer = null;
            commandInitializer = (builder, insertObj, suffix) => typedCommandInitializer.Invoke(this.DbParameters, this.OrmProvider, builder, insertObj, suffix);
            return (insertObjs, bulkCount, headSqlSetter, commandInitializer);
        }
    }

    public void Clear()
    {
        this.Tables?.Clear();
        this.TableAlias?.Clear();
        this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.TableAsStart = 'a';
        this.IsNeedAlias = false;
        this.deferredSegments.Clear();
        this.InsertFields.Clear();
    }

    public virtual void VisitWithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var insertObjType = insertObj.GetType();
        var cacheKey = HashCode.Combine(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType);
        var headSqlSetter = headSqlSetterCache.GetOrAdd(cacheKey, f =>
            RepositoryHelper.BuildCreateHeadSqlPart(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames));
        var commandInitializerCache = this.IsMultiple ? multiValuesSqlParametersCache : valuesSqlParametersCache;
        var commandInitializer = commandInitializerCache.GetOrAdd(cacheKey, f =>
            RepositoryHelper.BuildCreateValuesPartSqlParametes(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, this.IsMultiple));
        var fieldsBuilder = new StringBuilder();
        headSqlSetter.Invoke(fieldsBuilder, insertObj);
        var valuesBuilder = new StringBuilder();
        if (this.IsMultiple)
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, object, string>;
            typedCommandInitializer.Invoke(this.DbParameters, valuesBuilder, insertObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, object>;
            typedCommandInitializer.Invoke(this.DbParameters, valuesBuilder, insertObj);
        }
        this.InsertFields.Add(new FieldsSegment
        {
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }
    public virtual void VisitWithByField(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, memberMapper, parameterName, fieldValue);
        this.InsertFields.Add(new FieldsSegment
        {
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = parameterName
        });
    }
    public virtual string VisitFromWith(Delegate cteSubQuery, string cteTableName)
    {
        this.IsFromWith = true;
        var queryVisitor = this.CreateQueryVisitor(true);
        var fromQuery = new FromQuery(this.DbKey, this.OrmProvider, this.MapProvider, queryVisitor, this.IsParameterized);
        var query = cteSubQuery.DynamicInvoke(fromQuery) as IQueryBase;
        if (!queryVisitor.Equals(query.Visitor))
        {
            queryVisitor.Dispose();
            queryVisitor = query.Visitor;
        }
        var rawSql = queryVisitor.BuildSql(out var readerFields, false);
        rawSql = queryVisitor.BuildCteTableSql(cteTableName, rawSql, readerFields, query);
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder();
        builder.AppendLine(rawSql);
        string withTable = null;
        if (queryVisitor.SelfTableSegment != null)
            withTable = queryVisitor.SelfTableSegment.RefTableName;
        else if (!string.IsNullOrEmpty(cteTableName))
            withTable = cteTableName;
        else withTable = "import_data";
        queryVisitor.Dispose();

        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"INSERT INTO {tableName} (");
        var valuesBuilder = new StringBuilder(" SELECT ");

        for (int i = 0; i < readerFields.Count; i++)
        {
            var readerField = readerFields[i];
            if (!entityMapper.TryGetMemberMap(readerField.TargetMember.Name, out var memberMapper))
                continue;
            var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(fieldName);
            valuesBuilder.Append(fieldName);
        }
        fieldsBuilder.Append(')');
        valuesBuilder.Append($") FROM {this.OrmProvider.GetTableName(withTable)}");
        rawSql = fieldsBuilder.Append(valuesBuilder).ToString();
        fieldsBuilder.Clear();
        valuesBuilder.Clear();
        return rawSql;
    }
    public virtual string[] VisitFields(Expression fieldsSelector)
    {
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        this.InitTableAlias(lambdaExpr);
        var fieldNames = new List<string>();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        fieldNames.Add(memberMapper.MemberName);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        fieldNames.Add(memberMapper.MemberName);
                }
                break;
        }
        if (fieldNames.Count > 0)
            return fieldNames.ToArray();
        return null;
    }
    public void InitTableAlias(LambdaExpression lambdaExpr)
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
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.InsertFields = null;
    }
}