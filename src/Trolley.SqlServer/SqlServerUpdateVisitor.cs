using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    private static readonly ConcurrentDictionary<int, (object, object)> updateBulkWithCommandInitializerCache = new();

    public bool IsOutput { get; set; }
    public string OutputTableAlias { get; set; }
    public string OutputSql { get; set; }

    public SqlServerUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true)
    {
        if (!isMultiple)
        {
            this.Tables = new();
            this.TableAliases = new();
            var mapper = this.EntityMapProvider.GetEntityMap(entityType);
            this.Tables.Add(new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                //默认别名就是表名，在SetFrom时使用的别名就是表名
                AliasName = this.OrmProvider.GetTableName(mapper.TableName),
                Mapper = mapper
            });
        }
    }
    public override string BuildSql(DbContext dbContext, ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        var builder = new StringBuilder();
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    //此SQL只能用在多命令查询时和返回ToSql两个场景
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, readerFields) = this.BuildSetBulk(command);
                    Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";
                    Action<object, int> sqlExecute = null;
                    if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
                        index++;
                    }
                    sql = builder.ToString();
                }
                break;
            case ActionMode.Single:
                {
                    this.FieldsBuilder = new();
                    this.DbParameters ??= command.Parameters;
                    foreach (var deferredSegment in this.deferredSegments)
                    {
                        switch (deferredSegment.Type)
                        {
                            case "Set":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetFrom":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetWith":
                                this.VisitSetWith(deferredSegment.Value);
                                break;
                            case "SetFromField":
                                this.VisitSetFromField(deferredSegment.Value);
                                break;
                            case "Where":
                                this.VisitWhere(deferredSegment.Value as Expression);
                                break;
                            case "WhereWith":
                                this.VisitWhereWith(deferredSegment.Value);
                                break;
                            case "And":
                                this.VisitAnd(deferredSegment.Value as Expression);
                                break;
                            case "OutputFields":
                                this.VisitOutputFields(deferredSegment.Value as string);
                                break;
                            case "OutputExpression":
                                this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                                break;
                        }
                    }
                    readerFields = this.ReaderFields;
                    var aliasName = this.Tables[0].AliasName;
                    if (this.IsJoin)
                    {
                        builder.Append($"UPDATE {aliasName} SET ");
                        int index = 0;
                        if (this.FieldsBuilder.Count > 0)
                        {
                            foreach (var setField in this.FieldsBuilder)
                            {
                                if (index > 0) builder.Append(',');
                                builder.Append($"{aliasName}.");
                                builder.Append(setField);
                                index++;
                            }
                        }
                        builder.Append($" FROM {this.GetFormatTableName(this.Tables[0])} {aliasName}");
                        for (var i = 1; i < this.Tables.Count; i++)
                        {
                            var tableSegment = this.Tables[i];
                            var tableName = this.GetFormatTableName(tableSegment);
                            builder.Append($" {tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                            builder.Append($" ON {tableSegment.OnExpr}");
                        }
                    }
                    else
                    {
                        if (this.IsNeedTableAlias)
                            builder.Append($" {aliasName}");

                        int index = 0;
                        builder.Append(" SET ");
                        if (this.FieldsBuilder.Count > 0)
                        {
                            foreach (var setField in this.FieldsBuilder)
                            {
                                if (index > 0) builder.Append(',');
                                if (this.IsNeedTableAlias) builder.Append($"{aliasName}.");
                                builder.Append(setField);
                                index++;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(this.OutputSql))
                        builder.Append(this.OutputSql);
                    if (!string.IsNullOrEmpty(this.WhereBuilder))
                    {
                        builder.Append(" WHERE ");
                        builder.Append(this.WhereBuilder);
                    }
                    sql = builder.ToString();
                    builder.Clear();

                    if (this.IsJoin)
                    {
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                            sql = dbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                    }
                    else
                    {
                        Action<string> headSqlSetter = null;
                        var tableSchema = this.Tables[0].TableSchema;
                        if (!string.IsNullOrEmpty(tableSchema))
                            headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableSchema + "." + tableName)}");
                        else headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)}");
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                        {
                            var tableNames = this.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                headSqlSetter.Invoke(tableNames[i]);
                                builder.Append(sql);
                            }
                        }
                        else
                        {
                            var tableName = this.Tables[0].Mapper.TableName;
                            headSqlSetter.Invoke(this.Tables[0].Body ?? tableName);
                            builder.Append(sql);
                        }
                        sql = builder.ToString();
                    }
                }
                break;
        }
        builder.Clear();
        return sql;
    }
    public override (IEnumerable, int, string, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>,
        Action<StringBuilder, DbContext, string, object, string>, List<SqlFieldSegment>) BuildSetBulk(ITheaCommand command)
    {
        Type updateObjType = null;
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            break;
        }
        var builder = new StringBuilder();
        List<IDbDataParameter> fixedDbParameters = null;
        string fixedSql = null;
        int index = 0;
        if (this.deferredSegments.Count > 1)
        {
            this.DbParameters = new TheaDbParameterCollection();
            //先解析其他sql，生成固定sql
            this.FieldsBuilder = new();
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "Set":
                        this.VisitSet(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetWith":
                        this.VisitSetWith(deferredSegment.Value);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Output操作");
                }
            }
            if (this.FieldsBuilder.Count > 0)
            {
                foreach (var setField in this.FieldsBuilder)
                {
                    if (index > 0) builder.Append(',');
                    builder.Append(setField);
                    index++;
                }
                builder.Append(',');
                fixedSql = builder.ToString();
            }
            if (this.DbParameters.Count > 0)
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            this.DbParameters = command.Parameters;
            this.FieldsBuilder.Clear();
            builder.Clear();
        }
        //多命令查询时，第二次以后，DbParameters有值，不能再赋值
        else this.DbParameters ??= command.Parameters;

        builder.Append("UPDATE ");
        var tableSegment = this.Tables[0];
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            builder.Append($" {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.");
        var headSql = builder.ToString();
        var entityType = tableSegment.EntityType;

        //处理有tableSchema的场景
        Action<IDataParameterCollection> fixedParametersSetter = null;
        if (fixedDbParameters != null)
            fixedParametersSetter = dbParameters => fixedDbParameters.ForEach(f => dbParameters.Add(f));
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string> firstSqlSetter = null;
        Action<StringBuilder, DbContext, string, object, string> sqlSetter = null;
        bool isOutputSql = !string.IsNullOrEmpty(this.OutputSql);
        (var bulkSqlSetter, var shardingSqlSetter) = BuildUpdateBulkSetWithSqlParametersPart(this.DbContext, entityType, updateObjType, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames, isOutputSql);

        if (isOutputSql)
        {
            var typedBulkSqlSetter = bulkSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>;
            var typedShardingSqlSetter = shardingSqlSetter as Action<StringBuilder, DbContext, string, object, string>;
            firstSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                typedBulkSqlSetter.Invoke(dbParameters, builder, dbContext, this.OutputSql, updateObj, suffix);
            };
            sqlSetter = (builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                typedShardingSqlSetter.Invoke(builder, dbContext, this.OutputSql, updateObj, suffix);
            };
        }
        else
        {
            var typedBulkSqlSetter = bulkSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var typedShardingSqlSetter = shardingSqlSetter as Action<StringBuilder, DbContext, object, string>;
            firstSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                typedBulkSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
            };
            sqlSetter = (builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                typedShardingSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
            };
        }
        var tableName = tableSegment.Mapper.TableName;
        return (updateObjs, bulkCount, tableName, fixedParametersSetter, firstSqlSetter, sqlSetter, this.ReaderFields);
    }
    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT a.name FROM sys.objects a,sys.schemas b WHERE a.schema_id=b.schema_id AND A.type='U' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.name LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.name='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
    public override void SetFrom(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
    }
    public override void SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFromField",
            Value = (fieldSelector, valueSelector)
        });
    }
    public override void Join(string joinType, Type entityType, Expression joinOn)
    {
        this.Tables[0].AliasName = "a";
        base.Join(joinType, entityType, joinOn);
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

            if (memberExpr.HasParameter(out var parameterName))
            {
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.TableAliases[parameterName];
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
                if (this.IsOutput) fieldName = this.OutputTableAlias + "." + fieldName;
                else if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
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
        if (this.IsOutput)
        {
            var lambdaExpr = sqlSegment.Expression as LambdaExpression;
            var newExpr = lambdaExpr.Body as NewExpression;
            if (newExpr.Type.Name.StartsWith("<>"))
            {
                this.InitTableAlias(sqlSegment.Expression as LambdaExpression);
                var readerFields = new List<SqlFieldSegment>();
                this.ReaderFields = readerFields;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var fieldSqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.WrapSql(fieldSqlSegment, true);
                    if (fieldSqlSegment.IsConstant || fieldSqlSegment.IsVariable || fieldSqlSegment.HasParameter || fieldSqlSegment.IsExpression
                        || fieldSqlSegment.IsMethodCall || fieldSqlSegment.FromMember != null && fieldSqlSegment.FromMember.Name != memberInfo.Name)
                        fieldSqlSegment.IsNeedAlias = true;
                    fieldSqlSegment.TargetMember = memberInfo;
                    fieldSqlSegment.SegmentType = memberInfo.GetMemberType();
                    readerFields.Add(fieldSqlSegment);
                }
                return sqlSegment.ChangeValue(readerFields);
            }
        }
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        if (this.IsOutput)
        {
            var lambdaExpr = sqlSegment.Expression as LambdaExpression;
            var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
            var readerFields = new List<SqlFieldSegment>();
            this.ReaderFields = readerFields;
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                var memberInfo = memberAssignment.Member;
                var fieldSqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                this.WrapSql(fieldSqlSegment, true);
                if (fieldSqlSegment.IsConstant || fieldSqlSegment.IsVariable || fieldSqlSegment.HasParameter || fieldSqlSegment.IsExpression
                    || fieldSqlSegment.IsMethodCall || fieldSqlSegment.FromMember != null && fieldSqlSegment.FromMember.Name != memberInfo.Name)
                    fieldSqlSegment.IsNeedAlias = true;
                fieldSqlSegment.TargetMember = memberInfo;
                fieldSqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(fieldSqlSegment);
            }
            return sqlSegment.ChangeValue(readerFields);
        }
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public void Output(string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = fieldNames
        });
    }
    public void Output(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
    public void WithBulkCopy(IEnumerable updateObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (updateObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
    public void VisitOutputFields(string fieldNames)
    {
        this.ReaderFields = new();
        var entityType = this.Tables[0].EntityType;
        var upperFieldNames = fieldNames.ToUpper();
        if (fieldNames == "*")
        {
            upperFieldNames = "INSERTED.*";
            fieldNames = "INSERTED.*";
        }
        this.OutputSql = $" OUTPUT {fieldNames}";

        if (upperFieldNames == "INSERTED.*" || upperFieldNames == "DELETED.*")
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
                    MappedTargetType = memberMapper.MappedTargetType,
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
        this.IsOutput = true;
        this.ReaderFields = new();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" OUTPUT ");
        this.InitTableAlias(fieldsSelector);
        switch (fieldsSelector.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = fieldsSelector.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.WrapSql(sqlSegment, true);
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
                    this.WrapSql(sqlSegment, true);
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
                    this.WrapSql(sqlSegment, true);
                    sqlSegment.TargetMember = memberAssignment.Member;
                    sqlSegment.SegmentType = memberAssignment.Member.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.Parameter:
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
                        MappedTargetType = memberMapper.MappedTargetType,
                        TypeHandler = memberMapper.TypeHandler,
                        Body = memberMapper.FieldName
                    });
                }
                builder.Append("INSERTED.*");
                break;
            default:
                this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsSelector });
                for (int i = 0; i < this.ReaderFields.Count; i++)
                {
                    var readerField = this.ReaderFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(readerField.Body);
                    if (readerField.IsNeedAlias || readerField.IsConstant || readerField.IsVariable || readerField.HasParameter || readerField.IsExpression || readerField.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                }
                break;
        }
        this.OutputSql = builder.ToString();
        this.IsOutput = false;
        builder.Clear();
    }
    private static (object, object) BuildUpdateBulkSetWithSqlParametersPart(DbContext dbContext, Type entityType, Type updateObjType, bool isMultiple, bool isUpdateRowVersion, List<string> onlyFieldNames, List<string> ignoreFieldNames, bool hasFixedSql = false)
    {
        var ormProvider = dbContext.OrmProvider;
        var mapProvider = dbContext.EntityMapProvider;
        var cacheKey = RepositoryHelper.GetCacheKey(ormProvider.OrmProviderType, dbContext.EntityMapProvider, entityType, updateObjType, onlyFieldNames, ignoreFieldNames);
        return updateBulkWithCommandInitializerCache.GetOrAdd(cacheKey, f =>
        {
            var fieldsSetter = RepositoryHelper.BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 1, 2, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var whereSetter = RepositoryHelper.BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 1, false, true, true, false, isMultiple, true, " WHERE ") as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            var fieldsSqlSetter = RepositoryHelper.BuildFieldsSqlParametersPart(dbContext, entityType, updateObjType, 4, 2, 2, false, true, isUpdateRowVersion, onlyFieldNames, ignoreFieldNames) as Action<StringBuilder, DbContext, object, string>;
            var whereSqlSetter = RepositoryHelper.BuildWhereSqlParametersPart(dbContext, entityType, updateObjType, 2, false, true, true, false, isMultiple, true, " WHERE ") as Action<StringBuilder, DbContext, object, string>;
            object firstSqlSetter = null;
            object shardingSqlSetter = null;
            if (hasFixedSql)
            {
                Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string> typedFirstSqlSetter = (dbParameters, builder, dbContext, outputSql, updateObj, suffix) =>
                {
                    fieldsSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                    builder.Append(outputSql);
                    whereSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                };
                Action<StringBuilder, DbContext, string, object, string> typedShardingSqlSetter = (builder, dbContext, outputSql, updateObj, suffix) =>
                {
                    fieldsSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                    builder.Append(outputSql);
                    whereSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                };
                firstSqlSetter = typedFirstSqlSetter;
                shardingSqlSetter = typedShardingSqlSetter;
            }
            else
            {
                Action<IDataParameterCollection, StringBuilder, DbContext, object, string> typedFirstSqlSetter = (dbParameters, builder, dbContext, updateObj, suffix) =>
                {
                    fieldsSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                    whereSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                };
                Action<StringBuilder, DbContext, object, string> typedShardingSqlSetter = (builder, dbContext, updateObj, suffix) =>
                {
                    fieldsSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                    whereSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                };
                firstSqlSetter = typedFirstSqlSetter;
                shardingSqlSetter = typedShardingSqlSetter;
            }
            return (firstSqlSetter, shardingSqlSetter);
        });
    }
}