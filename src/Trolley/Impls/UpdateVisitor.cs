using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class UpdateVisitor : SqlVisitor, IUpdateVisitor
{
    public bool isFrom = false;
    public bool isJoin = false;
    public string whereSql = string.Empty;
    public List<SetField> setFields = new();
    public string fixedSql = null;
    public List<IDbDataParameter> fixedDbParameters = new();
    public Action<IDbCommand, UpdateVisitor, StringBuilder, object, int> bulkSetFieldsInitializer = null;

    public UpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType),
            AliasName = tableAsStart.ToString()
        });
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        var aliasName = this.tables[0].AliasName;
        if (this.IsNeedAlias && aliasName != this.TableAsStart.ToString())
            builder.Append($"{aliasName} ");

        if (this.isJoin && this.tables.Count > 1)
        {
            for (var i = 1; i < this.tables.Count; i++)
            {
                var tableSegment = this.tables[i];
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

        builder.Append("SET ");
        if (this.setFields != null && this.setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                var setField = this.setFields[i];
                if (i > 0) builder.Append(',');
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
            }
        }

        if (this.isFrom && this.tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.tables.Count; i++)
            {
                var tableSegment = this.tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }

        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(" WHERE " + this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public virtual IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.isFrom = true;
        int tableIndex = this.TableAsStart + this.tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
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
        this.isJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTable = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)('a' + this.tables.Count)}",
            JoinType = joinType
        };
        this.tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldsAssignment)
    {
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
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

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
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
                    this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
                }
                break;
        }
        return this;
    }
    public virtual IUpdateVisitor Set(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        this.AddMemberElement(memberMapper, fieldValue);
        return this;
    }
    public virtual IUpdateVisitor SetRaw(string rawSql, object parameters)
    {
        this.setFields.Add(new SetField { Type = SetFieldType.RawSql, Value = rawSql });
        var entityType = this.tables[0].EntityType;
        var dbParametersInitializer = RepositoryHelper.BuildRawSqlParameters(this.DbKey, this.OrmProvider, "SetRaw", rawSql, entityType, parameters);
        dbParametersInitializer.Invoke(this.dbParameters, this.OrmProvider, parameters);
        return this;
    }
    public virtual IUpdateVisitor SetWith(Expression fieldsAssignment)
    {
        var entityMapper = this.tables[0].Mapper;
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

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
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
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
        }
        return this;
    }
    public virtual IUpdateVisitor SetWith(Expression fieldsSelectorOrAssignment, object updateObj)
    {
        var entityMapper = this.tables[0].Mapper;
        if (fieldsSelectorOrAssignment != null)
        {
            var lambdaExpr = fieldsSelectorOrAssignment as LambdaExpression;
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        var memberExpr = lambdaExpr.Body as MemberExpression;
                        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                        this.AddMemberElement(memberMapper, updateObj);
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
                            this.AddMemberElement(memberMapper, updateObj);
                        else this.AddMemberElement(sqlSegment, memberMapper);
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
                            this.AddMemberElement(memberMapper, updateObj);
                        else this.AddMemberElement(sqlSegment, memberMapper);
                    }
                    break;
            }
            if (updateObj != null)
            {
                this.dbParameters ??= new();
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    var fieldValue = this.EvaluateAndCache(updateObj, keyMapper.MemberName);
                    if (fieldValue != null)
                    {
                        var parameterName = this.OrmProvider.ParameterPrefix + keyMapper.MemberName;
                        if (this.dbParameters.Exists(f => f.ParameterName == parameterName))
                            parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
                        this.dbParameters.Add(this.OrmProvider.CreateParameter(keyMapper, parameterName, fieldValue));
                        if (!string.IsNullOrEmpty(this.whereSql))
                            this.whereSql += " AND ";
                        var aliasName = this.tables[0].AliasName;
                        if (this.IsNeedAlias && aliasName != this.TableAsStart.ToString())
                            this.whereSql += $"{aliasName}.";
                        this.whereSql += $"{this.OrmProvider.GetFieldName(keyMapper.FieldName)}={parameterName}";
                    }
                }
            }
        }
        else
        {
            var parametersInitializer = RepositoryHelper.BuildUpdateSetWithParameters(this, entityMapper.EntityType, updateObj);
            parametersInitializer.Invoke(this, this.setFields, this.dbParameters, updateObj);
        }
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldsAssignment)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.tables[0].Mapper;
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
                        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
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
                        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
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
        return this;
    }
    public virtual IUpdateVisitor SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.IsNeedAlias = true;
        var entityMapper = this.tables[0].Mapper;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

        this.InitTableAlias(lambdaExpr);
        var sql = this.VisitFromQuery(valueSelector as LambdaExpression, out var isNeedAlias);
        if (isNeedAlias) this.IsNeedAlias = true;
        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
        //this.AddMemberElement(memberMapper, fieldValue);

        //var setFields = new List<SetField>();
        //switch (lambdaExpr.Body.NodeType)
        //{
        //    case ExpressionType.New:
        //        this.InitTableAlias(lambdaExpr);
        //        var newExpr = lambdaExpr.Body as NewExpression;
        //        for (int i = 0; i < newExpr.Arguments.Count; i++)
        //        {
        //            var memberInfo = newExpr.Members[i];
        //            if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
        //                continue;

        //            var argumentExpr = newExpr.Arguments[i];
        //            if (argumentExpr.GetParameters(out var argumentParameters)
        //                && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
        //            {
        //                var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
        //                var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
        //                if (isNeedAlias) this.IsNeedAlias = true;
        //                setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
        //            }
        //            else
        //            {
        //                var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
        //                //只一个成员访问，没有设置语句，什么也不做，忽略
        //                if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
        //                    continue;
        //                setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
        //            }
        //        }
        //        break;
        //    case ExpressionType.MemberInit:
        //        this.InitTableAlias(lambdaExpr);
        //        var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
        //        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        //        {
        //            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
        //            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
        //                continue;

        //            var argumentExpr = memberAssignment.Expression;
        //            if (argumentExpr.GetParameters(out var argumentParameters)
        //                && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
        //            {
        //                var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
        //                var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
        //                if (isNeedAlias) this.IsNeedAlias = true;
        //                setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
        //            }
        //            else
        //            {
        //                var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
        //                //只一个成员访问，没有设置语句，什么也不做，忽略
        //                if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
        //                    continue;
        //                setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
        //            }
        //        }
        //        break;
        //}
        //if (setFields != null && setFields.Count > 0)
        //{
        //    for (int i = 0; i < setFields.Count; i++)
        //    {
        //        if (i > 0) builder.Append(',');
        //        if (this.IsNeedAlias)
        //            builder.Append("a.");
        //        builder.Append($"{this.OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
        //    }
        //}
        //this.setSql = builder.ToString();
        return this;
    }
    public virtual IUpdateVisitor SetBulkFirst(Expression fieldsSelectorOrAssignment, object updateObjs)
    {
        var entityMapper = this.tables[0].Mapper;
        if (fieldsSelectorOrAssignment != null)
        {
            var fixedBuilder = new StringBuilder();
            var lambdaExpr = fieldsSelectorOrAssignment as LambdaExpression;
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        var memberExpr = lambdaExpr.Body as MemberExpression;
                        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberExpr.Member.Name}";
                        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
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
                            this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
                        }
                        else
                        {
                            var sqlOrParameterName = this.GetQuotedValue(sqlSegment, this.fixedDbParameters);
                            if (fixedBuilder.Length > 0) fixedBuilder.Append(',');
                            fixedBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={sqlOrParameterName}");
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

                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberMapper.MemberName)
                        {
                            var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                            this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
                        }
                        else
                        {
                            var sqlOrParameterName = this.GetQuotedValue(sqlSegment, this.fixedDbParameters);
                            if (fixedBuilder.Length > 0) fixedBuilder.Append(',');
                            fixedBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={sqlOrParameterName}");
                        }
                    }
                    break;
            }

            this.fixedSql = $"UPDATE {this.OrmProvider.GetTableName(entityMapper.TableName)} SET ";
            if (fixedBuilder.Length > 0)
                this.fixedSql += fixedBuilder.ToString();

            this.bulkSetFieldsInitializer = (command, visitor, builder, updateObj, index) =>
            {
                builder.Append(visitor.fixedSql);
                dbParameters.AddRange(visitor.fixedDbParameters);
                foreach (var setField in visitor.setFields)
                {
                    var fieldValue = visitor.EvaluateAndCache(updateObj, setField.MemberMapper.MemberName);
                    builder.Append($"{visitor.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}{index}");
                    dbParameters.Add(visitor.OrmProvider.CreateParameter(setField.MemberMapper, $"{setField.Value}{index}", fieldValue));
                }
                builder.Append(" WHERE ");
                for (int i = 0; i < entityMapper.KeyMembers.Count; i++)
                {
                    var keyMember = entityMapper.KeyMembers[i];
                    if (i > 0) builder.Append(" AND ");
                    var parameterName = $"{visitor.OrmProvider.ParameterPrefix}k{keyMember.MemberName}{index}";
                    builder.Append($"{visitor.OrmProvider.GetFieldName(keyMember.FieldName)}={parameterName}");
                    var fieldValue = visitor.EvaluateAndCache(updateObj, keyMember.MemberName);
                    dbParameters.Add(visitor.OrmProvider.CreateParameter(keyMember, parameterName, fieldValue));
                }
            };
        }
        else this.bulkSetFieldsInitializer = RepositoryHelper.BuildUpdateBulkSetFieldsParameters(this, entityMapper.EntityType, updateObjs);
        return this;
    }
    public virtual void SetBulk(StringBuilder builder, IDbCommand command, object updateObj, int index)
        => this.bulkSetFieldsInitializer.Invoke(command, this, builder, updateObj, index);
    public virtual IUpdateVisitor Where(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.lastWhereNodeType = OperationType.None;
        this.whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public virtual IUpdateVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            this.whereSql = $"({this.whereSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            this.whereSql += " AND " + conditionSql;
        else this.whereSql = conditionSql;
        this.isWhere = false;
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
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                return formatter.Invoke(this, targetSegment);
            }

            if (memberExpr.IsParameter(out var parameterName))
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
                var tableSegment = this.tableAlias[parameterName];
                tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);

                if (memberMapper.IsIgnore)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                if (memberMapper.MemberType.IsEnumType(out var expectType, out _))
                {
                    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                    if (targetType == typeof(string))
                    {
                        sqlSegment.ExpectType = expectType;
                        sqlSegment.TargetType = targetType;
                    }
                }

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (this.IsNeedAlias)
                    fieldName = tableSegment.AliasName + "." + fieldName;

                sqlSegment.HasField = true;
                sqlSegment.IsConstant = false;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.MemberMapper = memberMapper;
                sqlSegment.Value = fieldName;
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.IsExpression = false;
        sqlSegment.IsMethodCall = false;
        return sqlSegment;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{newExpr}");
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        if (memberInitExpr.IsParameter(out _))
            throw new NotSupportedException($"不支持的表达式访问,{memberInitExpr}");
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
        this.tableAlias.Clear();
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
            this.tableAlias.Add(parameterExpr.Name, this.tables[index]);
            index++;
        }
    }
    protected void AddMemberElement(MemberMap memberMapper, object memberValue)
    {
        if (memberValue is DBNull || memberValue == null)
        {
            this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        var fieldValue = this.EvaluateAndCache(memberValue, memberMapper.MemberName);
        this.dbParameters ??= new();
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.dbParameters.Exists(f => f.ParameterName == parameterName))
            parameterName = this.OrmProvider.ParameterPrefix + this.parameterPrefix + this.dbParameters.Count.ToString();
        this.dbParameters.Add(this.OrmProvider.CreateParameter(memberMapper, parameterName, fieldValue));
        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = parameterName });
    }
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        this.setFields.Add(new SetField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
    }
}
