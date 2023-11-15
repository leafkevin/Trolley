using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    protected List<CommandSegment> deferredSegments = new();

    protected List<InsertField> InsertFields { get; set; } = new();
    protected virtual bool IsBulk { get; set; } = false;

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
    public virtual string BuildCommand(IDbCommand command)
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
                    //case "SetObject":
                    //    this.VisitSet(command, deferredSegment.Value);
                    //    break;
                    //case "SetExpression":
                    //    this.VisitSet(deferredSegment.Value as Expression);
                    //    break;
            }
        }
        if (!this.IsBulk)
        {
            sql = this.BuildSql();
            command.CommandText = sql;
        }
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
    public override int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        int result = 1;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        sqlBuilder.Append(this.BuildCommand(command));
        return result;
    }
    public virtual string BuildSql()
    {
        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
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

        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            valuesBuilder.Append(tailSql);
        fieldsBuilder.Append(valuesBuilder);
        return fieldsBuilder.ToString();
    }
    public virtual string BuildHeadSql() => $"INSERT INTO";
    public virtual string BuildTailSql() => string.Empty;
    public virtual ICreateVisitor WithBy(object insertObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBy",
            Value = insertObj
        });
        return this;
    }
    public virtual ICreateVisitor WithByField(FieldObject fieldObject)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByField",
            Value = fieldObject
        });
        return this;
    }
    public virtual ICreateVisitor WithBulk(object insertObjs)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulk",
            Value = insertObjs
        });
        return this;
    }
    public virtual string BuildBulkSql(IDbCommand command)
    {
        int index = 0;
        this.IsBulk = true;
        var builder = new StringBuilder();
        var insertObjs = this.deferredSegments[0].Value as IEnumerable;
        this.BuildBulkHeadSql(builder, out var dbParametersInitializer);
        if (this.IsMultiple)
        {
            var bulkCommandInitializer = dbParametersInitializer as Action<IDataParameterCollection, StringBuilder, string, object, int>;
            foreach (var insertObj in insertObjs)
            {
                this.WithBulk(command, builder, bulkCommandInitializer, insertObj, index);
                index++;
            }
        }
        else
        {
            var bulkCommandInitializer = dbParametersInitializer as Action<IDataParameterCollection, StringBuilder, object, int>;
            foreach (var insertObj in insertObjs)
            {
                this.WithBulk(command, builder, bulkCommandInitializer, insertObj, index);
                index++;
            }
        }
        this.WithBulkTail(builder);
        return builder.ToString();
    }
    public virtual string BuildBulkHeadSql(StringBuilder builder, out object dbParametersInitializer)
    {
        var entityType = this.Tables[0].EntityType;
        var insertObjs = this.deferredSegments[0].Value;
        dbParametersInitializer = RepositoryHelper.BuildCreateWithBulkDbParametersInitializer(this, entityType, insertObjs, this.IsMultiple, out var bulkHeadSql);
        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var headSql = $"{this.BuildHeadSql()} {tableName} ({bulkHeadSql}) VALUES ";
        builder.Append(headSql);
        return headSql;
    }
    public virtual void WithBulk(IDbCommand command, StringBuilder builder, Action<IDataParameterCollection, StringBuilder, object, int> dbParametersInitializer, object insertObj, int index)
    {
        if (index > 0) builder.Append(',');
        builder.Append('(');
        dbParametersInitializer.Invoke(command.Parameters, builder, insertObj, index);
        builder.Append(')');
    }
    public virtual void WithBulk(IDbCommand command, StringBuilder builder, Action<IDataParameterCollection, StringBuilder, string, object, int> dbParametersInitializer, object insertObj, int index)
    {
        if (index > 0) builder.Append(',');
        builder.Append('(');
        dbParametersInitializer.Invoke(command.Parameters, builder, $"m{this.CommandIndex}", insertObj, index);
        builder.Append(')');
    }
    public virtual void WithBulkTail(StringBuilder builder)
    {
        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            builder.Append(tailSql);
    }
    public virtual IQueryVisitor CreateQuery(params Type[] sourceTypes)
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        queryVisiter.From(this.TableAsStart, sourceTypes);
        return queryVisiter;
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
        var dbParametersInitializer = RepositoryHelper.BuildCreateWithBiesDbParametersInitializer(this, entityType, insertObj, this.IsMultiple);
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        if (this.IsMultiple)
        {
            var multiCommandInitializer = dbParametersInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, string, object>;
            multiCommandInitializer.Invoke(this.DbParameters, fieldsBuilder, valuesBuilder, $"_m{this.CommandIndex}", insertObj);
        }
        else
        {
            var singleCommandInitializer = dbParametersInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, object>;
            singleCommandInitializer.Invoke(this.DbParameters, fieldsBuilder, valuesBuilder, insertObj);
        }
        this.InsertFields.Add(new InsertField
        {
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }
    public virtual void VisitWithByField(FieldObject fieldObject)
    {
        var lambdaExpr = fieldObject.FieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        var addDbParametersDelegate = RepositoryHelper.BuildAddDbParameters(this.DbKey, this.OrmProvider, memberMapper, fieldObject.FieldValue);
        addDbParametersDelegate.Invoke(this.DbParameters, this.OrmProvider, parameterName, fieldObject.FieldValue);
        this.InsertFields.Add(new InsertField
        {
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = parameterName
        });
    }

    //protected virtual void VisitIfNotExists(IDbCommand command, object whereObj)
    //{
    //    this.IsUseIfNotExists = true;
    //    if (whereObj is Expression whereExpr)
    //    {
    //        this.IsWhere = true;
    //        var lambdaExpr = whereExpr as LambdaExpression;
    //        this.InitTableAlias(lambdaExpr);
    //        this.LastWhereNodeType = OperationType.None;
    //        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
    //        this.IsWhere = false;
    //    }
    //    else
    //    {
    //        var dbParametersInitializer = RepositoryHelper.BuildWhereWithKeysSqlParameters(this, this.Tables[0].EntityType, whereObj, this.IsMultiple);
    //        if (this.IsMultiple)
    //        {
    //            var multiCommandInitializer = dbParametersInitializer as Func<IDbCommand, object, int, string>;
    //            this.WhereSql = multiCommandInitializer.Invoke(command, whereObj, this.CommandIndex);
    //        }
    //        else
    //        {
    //            var singleCommandInitializer = dbParametersInitializer as Func<IDbCommand, object, string>;
    //            this.WhereSql = singleCommandInitializer.Invoke(command, whereObj);
    //        }
    //    }
    //}   
}
public struct InsertField
{
    public string Fields { get; set; }
    public string Values { get; set; }
}