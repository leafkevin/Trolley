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
    protected List<CommandSegment> deferredSegments = new();
    protected bool hasOnlyFields = false;
    protected bool hasIgnoreFields = false;

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsFrom { get; set; }
    public bool IsJoin { get; set; }
    public StringBuilder FieldsBuilder { get; set; }
    public bool HasWhere { get; protected set; }
    public bool IsNeedShardingValues { get; set; }
    public Dictionary<string, object> ShardingValues { get; set; }


    public UpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
    {
        this.DbContext = dbContext;
        this.TableAliasStart = tableAsStart;
        this.Command = command ?? dbContext.OrmProvider.CreateCommand();
        this.DbParameters = this.Command.Parameters;
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
    public virtual string BuildSql(ITheaCommand command, out List<SqlSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        this.hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        if (this.HasWhere) this.WhereBuilder = new();

        var builder = new StringBuilder();
        var tableSegment = this.Tables[0];
        var shardingType = tableSegment.ShardingType;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding)
        {
            if (tableSegment.ShardingTableGetter == null)
            {
                if (tableSegment.TableShardingInfo.DependOnMembers == null || tableSegment.TableShardingInfo.DependOnMembers.Count == 0)
                    throw new Exception($"实体表{tableSegment.EntityType.FullName}已设置分表，未指定分表，也未设置依赖字段无法确定分表，请使用UseTable/UseTableBy方法手动指定分表");
                if (this.deferredSegments.Count > 1)
                {
                    this.IsNeedShardingValues = true;
                    this.ShardingValues = new();
                }
            }
            else if (this.ActionMode == ActionMode.Single && tableSegment.ShardingTableGetter != null)
            {
                var updateObj = this.deferredSegments[0].Value;
                tableSegment.Body = tableSegment.ShardingTableGetter.Invoke(updateObj);
                tableSegment.ShardingType = ShardingTableType.SingleTable;
                tableSegment.IsSharding = true;
            }
        }
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                (shardingType, shardingTables, var updateObjs, _, var fixedSqlSetter,
                    var loopSqlSetter, readerFields) = this.BuildSetBulk(command);

                int index = 0;
                fixedSqlSetter?.Invoke(command.Parameters);
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledUpdateObjs.Keys)
                    {
                        var tableParameters = tabledUpdateObjs[tableName];
                        foreach (var updateObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                            index++;
                        }
                    }
                }
                else
                {
                    foreach (var updateObj in updateObjs)
                    {
                        switch (shardingType)
                        {
                            case ShardingTableType.None:
                            case ShardingTableType.SingleTable:
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                break;
                            case ShardingTableType.MultiTable:
                            case ShardingTableType.ShardingTableMap:
                                var tableNames = shardingTables as List<string>;
                                foreach (var tableName in tableNames)
                                {
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                }
                                break;
                        }
                        index++;
                    }
                }
                sql = builder.ToString();
                break;
            case ActionMode.Single:
                this.FieldsBuilder = new();
                var entityType = tableSegment.EntityType;
                Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
                foreach (var deferredSegment in this.deferredSegments)
                {
                    switch (deferredSegment.Type)
                    {
                        case "SetExpr":
                            this.VisitSetExpr(deferredSegment.Value as Expression);
                            break;
                        case "SetFrom":
                            this.VisitSetExpr(deferredSegment.Value as Expression);
                            break;
                        case "SetField":
                            this.VisitSetField(deferredSegment.Value);
                            break;
                        case "SetFieldExpr":
                            this.VisitSetFieldExpr(deferredSegment.Value);
                            break;
                        case "SetObject":
                            this.VisitSetObject(deferredSegment.Value);
                            break;
                        case "SetFromField":
                            this.VisitSetFromField(deferredSegment.Value);
                            break;
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
                        default: throw new NotSupportedException("Set操作后，只支持Set/SetFrom/IgnoreFields/OnlyFields/Where/And/Or操作");
                    }
                }
                builder.Append($"UPDATE {this.GetFormatTableName(tableSegment)}");
                if (this.IsNeedTableAlias) builder.Append($" {this.Tables[0].AliasName}");
                if (this.IsJoin)
                {
                    for (var i = 1; i < this.Tables.Count; i++)
                    {
                        var myTableSegment = this.Tables[i];
                        var tableName = this.GetFormatTableName(myTableSegment);
                        builder.Append($" {myTableSegment.JoinType} {tableName} {myTableSegment.AliasName} ON {myTableSegment.OnExpr}");
                    }
                }

                builder.Append(" SET ");
                builder.Append(this.FieldsBuilder.ToString());
                if (this.WhereBuilder != null && this.WhereBuilder.Length > 0)
                {
                    builder.Append(" WHERE ");
                    builder.Append(this.WhereBuilder);
                }
                sql = builder.ToString();
                if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                    sql = this.DbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                break;
        }
        builder.Clear();
        return sql;
    }
    public virtual (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>,
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>, List<SqlSegment>) BuildSetBulk(ITheaCommand command)
    {
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;

        object firstUpdateObj = null;
        foreach (var updateObj in updateObjs)
        {
            firstUpdateObj = updateObj;
            break;
        }
        var updateObjType = firstUpdateObj.GetType();
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;

        var headSql = "UPDATE";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += $" {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.";
        var fixedHeadSql = "SET ";
        var fixedTailSql = ";";

        List<IDbDataParameter> fixedDbParameters = null;
        Action<IDataParameterCollection> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string> loopSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            this.FieldsBuilder = new();
            var tempDbParameters = new TheaDbParameterCollection();
            this.DbParameters = tempDbParameters;
            Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "SetExpr":
                        this.VisitSetExpr(deferredSegment.Value as Expression);
                        break;
                    case "SetFrom":
                        this.VisitSetExpr(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetFieldExpr":
                        this.VisitSetFieldExpr(deferredSegment.Value);
                        break;
                    case "SetObject":
                        this.VisitSetObject(deferredSegment.Value);
                        break;
                    case "SetFromField":
                        this.VisitSetFromField(deferredSegment.Value);
                        break;
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
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/SetFrom/IgnoreFields/OnlyFields/Where/And/Or操作");
                }
            }
            if (this.DbParameters.Count > 0)
            {
                fixedDbParameters = tempDbParameters.ToList();
                firstSqlSetter = dbParameters => fixedDbParameters.ForEach(f => dbParameters.Add(f));
            }
            if (this.FieldsBuilder.Length > 0)
                fixedHeadSql = $"SET {this.FieldsBuilder.ToString()},";
            if (this.WhereBuilder.Length > 0)
                fixedTailSql = $" AND {this.WhereBuilder.ToString()};";
            this.DbParameters.Clear();
        }

        if (firstUpdateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            (var valueSetters, var whereSetters) = this.BuildDictBulkCommandInitializer(entityMapper, dict);
            loopSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, index) =>
            {
                var dictObj = updateObj as IDictionary<string, object>;
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fixedHeadSql}");
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, index.ToString());
                builder.Append(" WHERE ");
                foreach (var valueSetter in whereSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, index.ToString());
                builder.Append(fixedTailSql);
            };
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, updateObjType, 2, this.OnlyFieldNames, this.IgnoreFieldNames)
                as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            loopSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, index) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fixedHeadSql}");
                commandInitializer.Invoke(dbParameters, builder, dbContext, updateObj, index.ToString());
                builder.Append(fixedTailSql);
            };
        }

        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                shardingTables = shardingType switch
                {
                    ShardingTableType.SingleTable => tableSegment.Body,
                    ShardingTableType.MultiTable => tableSegment.TableNames,
                    _ => tableSegment.Mapper.TableName,
                };
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, updateObjType, updateObjs, firstUpdateObj, this.ShardingValues);
            }
        }
        return (shardingType, shardingTables, updateObjs, bulkCount, firstSqlSetter, loopSqlSetter, null);
    }
    public virtual void Join(string joinType, Type entityType, Expression joinOn)
    {
        this.IsNeedTableAlias = true;
        this.IsJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var aliasName = $"{(char)(this.TableAliasStart + this.Tables.Count)}";
        var joinTable = new TableSegment
        {
            TableType = TableType.Entity,
            EntityType = entityType,
            Mapper = this.EntityMapProvider.GetEntityMap(entityType),
            AliasName = aliasName,
            JoinType = joinType,
            Path = aliasName,
            IsMaster = true
        };
        this.Tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body, out _);
    }
    public virtual void SetObject(object updateObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetObject",
            Value = updateObj
        });
    }
    public virtual void SetExpr(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetExpr",
            Value = fieldsAssignment
        });
    }
    public virtual void SetField(string fieldName, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetField",
            Value = (fieldName, fieldValue)
        });
    }
    public virtual void SetField(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFieldExpr",
            Value = (fieldSelector, fieldValue)
        });
    }
    public virtual void SetFrom(Expression fieldsAssignment)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
    }
    public virtual void SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedTableAlias = true;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFromField",
            Value = (fieldSelector, valueSelector)
        });
    }
    public virtual void IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames ??= new();
        this.IgnoreFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
    {
        this.IgnoreFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.IgnoreFieldNames.Add(f.FieldName.ToLower()));
    }
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void OnlyFields(Expression fieldsSelector)
    {
        this.OnlyFieldNames ??= new();
        this.VisitFields(fieldsSelector, f => this.OnlyFieldNames.Add(f.FieldName.ToLower()));
    }
    public virtual void SetBulk(IEnumerable updateObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetBulk",
            Value = (updateObjs, bulkCount)
        });
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
                    sqlSegment.FieldName = memberMapper.FieldName;
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
                    sqlSegment.FieldName = readerField.FieldName;
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
        if (sqlSegment.Expression.HasParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.HasParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{sqlSegment.Expression}");
        //当作常量处理
        return sqlSegment.ChangeValue(sqlSegment.Expression.Evaluate(), true);
    }
    public override SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        //把方法返回值当作常量处理
        sqlSegment = base.VisitMethodCall(sqlSegment);
        if (!sqlSegment.HasField && !sqlSegment.HasParameter && !sqlSegment.IsMethodCall)
            sqlSegment.IsConstant = true;
        return sqlSegment;
    }
    public virtual void Clear()
    {
        this.Tables?.Clear();
        this.TableAliases?.Clear();
        this.ReaderFields?.Clear();
        this.WhereBuilder = null;
        //this.IsFromQuery = false;
        this.TableAliasStart = 'a';
        this.IsNeedTableAlias = false;

        this.IsFrom = false;
        this.IsJoin = false;
        this.deferredSegments.Clear();
        this.FieldsBuilder.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.FieldsBuilder = null;
        this.OnlyFieldNames = null;
        this.IgnoreFieldNames = null;
    }
    public virtual void VisitSetField(object deferredSegmentValue)
    {
        (var fieldName, var fieldValue) = ((string, object))deferredSegmentValue;
        var entityMapper = this.Tables[0].Mapper;
        if (!entityMapper.TryGetMemberMapByFieldName(fieldName, out var memberMapper))
            throw new Exception($"没有找到字段{fieldName}");
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.AddMemberElement(memberMapper, fieldValue, false);
    }
    public virtual void VisitSetFieldExpr(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.AddMemberElement(memberMapper, fieldValue, false);
    }
    public virtual void VisitSetObject(object updateObj)
    {
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var updateObjType = updateObj.GetType();

        if (updateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;

                var fieldValue = dict[key];
                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                if (this.hasOnlyFields || this.hasIgnoreFields)
                {
                    var lowerMemberName = memberMapper.MemberName.ToLower();
                    if (this.hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName)
                        || this.hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                        continue;
                }

                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (this.FieldsBuilder.Length > 0) this.FieldsBuilder.Append(',');
                this.FieldsBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

                if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
            }
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, updateObjType, 2, false, false, this.OnlyFieldNames, this.IgnoreFieldNames);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.DbContext, updateObj);
        }
        if (this.IsNeedShardingValues) RepositoryHelper.SetShardingValues(this.DbContext,
            tableSegment.TableShardingInfo, tableSegment.EntityType, updateObjType, updateObj, this.ShardingValues);
    }
    public virtual void VisitSetExpr(Expression fieldsAssignment)
    {
        var entityMapper = this.Tables[0].Mapper;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                        continue;

                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr.TryGetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        (var sql, _, _) = this.VisitFromQuery(newLambdaExpr);
                        this.FieldsBuilder.Append($"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
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
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                        continue;

                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr.TryGetParameters(out var argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        (var sql, _, _) = this.VisitFromQuery(newLambdaExpr);
                        this.FieldsBuilder.Append($"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
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
    public virtual void VisitSetFromField(object deferredSegmentValue)
    {
        var entityMapper = this.Tables[0].Mapper;
        (var fieldSelector, var valueSelector) = ((Expression, Expression))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        if (memberMapper.IsIgnore || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略更新，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreUpdate：{memberMapper.IsIgnoreUpdate}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许更新，IsRowVersion：{memberMapper.IsRowVersion}");

        this.InitTableAlias(valueSelector as LambdaExpression);
        (var sql, _, _) = this.VisitFromQuery(valueSelector as LambdaExpression);
        this.FieldsBuilder.Append($"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=({sql})");
    }
    public virtual void VisitAnd(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitAndSql(whereSql, operationType);
    }
    public virtual void VisitOr(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitOrSql(whereSql, operationType);
    }
    public virtual void VisitFields(Expression fieldsSelector, Action<MemberMap> fieldsAction)
    {
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        MemberMap memberMapper = null;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var memberExpr = lambdaExpr.Body as MemberExpression;
                memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                fieldsAction.Invoke(memberMapper);
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment
                    {
                        Expression = newExpr.Arguments[i],
                        NativeDbType = memberMapper.NativeDbType,
                        MappedTargetType = memberMapper.MappedTargetType,
                        TypeHandler = memberMapper.TypeHandler
                    });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        fieldsAction.Invoke(memberMapper);
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
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        fieldsAction.Invoke(memberMapper);
                }
                break;
        }
    }
    public virtual void AddMemberElement(MemberMap memberMapper, object memberValue, bool isEntity = true)
    {
        if (this.FieldsBuilder.Length > 0) this.FieldsBuilder.Append(',');
        if (memberValue is DBNull || memberValue == null)
        {
            this.FieldsBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=NULL");
            return;
        }
        var fieldValue = isEntity ? memberMapper.Member.Evaluate(memberValue) : memberValue;
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
        else
        {
            var targetType = memberMapper.MappedTargetType;
            var valueGetter = this.OrmProvider.GetParameterValueGetter(memberValue.GetType(), targetType, false, this.DbContext.Options);
            fieldValue = valueGetter.Invoke(fieldValue);
        }
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
        this.FieldsBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

        if (this.IsNeedShardingValues)
        {
            var tableShardingInfo = this.Tables[0].TableShardingInfo;
            if (!tableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName)) return;
            this.ShardingValues[memberMapper.MemberName] = fieldValue;
        }
    }
    public virtual void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (this.FieldsBuilder.Length > 0) this.FieldsBuilder.Append(',');
        if (sqlSegment == SqlSegment.Null)
        {
            this.FieldsBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}=NULL");
            return;
        }
        if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var fieldValue = sqlSegment.Value;
            var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
            this.FieldsBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

            if (this.IsNeedShardingValues)
            {
                var tableShardingInfo = this.Tables[0].TableShardingInfo;
                if (!tableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName)) return;
                this.ShardingValues[memberMapper.MemberName] = fieldValue;
            }
        }
    }
    public (List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, List<Action<IDataParameterCollection,
        StringBuilder, IDictionary<string, object>, string>>) BuildDictBulkCommandInitializer(EntityMap entityMapper, IDictionary<string, object> dict)
    {
        var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
        var whereSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
        foreach (var key in dict.Keys)
        {
            if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
               || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                continue;
            if (this.hasOnlyFields || this.hasIgnoreFields)
            {
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (this.hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName)
                    || this.hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                    continue;
            }

            Func<IDictionary<string, object>, object> valueGetter = null;
            if (memberMapper.TypeHandler != null)
                valueGetter = updateObj => memberMapper.TypeHandler.ToFieldValue(updateObj[key]);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var fieldValue = dict[key];
                if (memberMapper.IsRequired)
                {
                    if (fieldValue == null)
                        throw new Exception($"实体{entityMapper.EntityType.FullName}表，字段{memberMapper.FieldName}为必填，值不能为空");

                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType.ToUnderlyingType() != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                        valueGetter = updateObj => myValueGetter.Invoke(updateObj[key]);
                    }
                    else valueGetter = updateObj => updateObj[key];
                }
                else
                {
                    if (fieldValue != null)
                    {
                        var fieldValueType = dict[key].GetType();
                        if (fieldValueType.ToUnderlyingType() != targetType)
                        {
                            var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                            valueGetter = updateObj =>
                            {
                                var fieldValue = updateObj[key];
                                return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                            };
                        }
                        else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                    }
                    else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                }
            }

            Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
            if (memberMapper.IsKey)
            {
                if (whereSetters.Count > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append($" AND {this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                whereSetters.Add(valueSetter);
            }
            else
            {
                if (valueSetters.Count > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append($",{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
            }
        }
        return (valueSetters, whereSetters);
    }
    public virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.TryGetParameterNames(out var parameters);
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
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[index]);
            index++;
        }
    }
    public string GetTableName(TableSegment tableSegment)
    {
        string tableName = null;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding) tableName = tableSegment.Body;
            else tableName = RepositoryHelper.GetShardingTableName(this.DbContext, tableSegment.TableShardingInfo, this.ShardingValues);
        }
        else tableName = tableSegment.Mapper.TableName;
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        else tableName = this.OrmProvider.GetTableName(tableName);
        return tableName;
    }
}