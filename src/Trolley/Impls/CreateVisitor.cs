using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private string bulkHeadSql;
    private Action<IDbCommand, ISqlVisitor, StringBuilder, int, object> bulkCommandInitializer;

    protected readonly List<InsertField> insertFields = new();
    protected bool isFrom = false;
    protected bool isUseIgnore;
    protected bool isUseUpdate;
    protected object ignoreKeysOrUniqueKeys;


    public virtual bool IsBulk { get; set; } = false;

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", string multiParameterPrefix = "")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, multiParameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType)
        });
        this.dbParameters = new();
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var tableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
        var valuesBuilder = new StringBuilder();
        string fromTables = null;
        if (this.isFrom) valuesBuilder.Append(" SELECT ");
        else valuesBuilder.Append(" VALUES(");
        for (int i = 0; i < this.insertFields.Count; i++)
        {
            var insertField = this.insertFields[i];
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(insertField.Fields);
            valuesBuilder.Append(insertField.Values);
            if (insertField.Type == InsertFieldType.FromTables)
                fromTables = insertField.FromTables;
        }
        fieldsBuilder.Append(')');
        if (this.isFrom)
        {
            valuesBuilder.Append($" FROM {fromTables}");
            if (!string.IsNullOrEmpty(this.whereSql))
                valuesBuilder.Append(" WHERE " + this.whereSql);
        }
        else valuesBuilder.Append(')');

        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            valuesBuilder.Append(tailSql);
        fieldsBuilder.Append(valuesBuilder);
        dbParameters = this.dbParameters;
        return fieldsBuilder.ToString();
    }
    public virtual string BuildHeadSql() => $"INSERT INTO";
    public virtual string BuildTailSql()
    {
        if (!this.IsBulk && this.tables[0].Mapper.IsAutoIncrement)
            return ";SELECT LAST_INSERT_ID()";
        return string.Empty;
    }
    public virtual ICreateVisitor UseIgnore(object keysOrUniqueKeys = null)
    {
        this.ignoreKeysOrUniqueKeys = keysOrUniqueKeys;
        this.isUseIgnore = true;
        return this;
    }
    public virtual ICreateVisitor UseUpdate()
    {
        this.isUseUpdate = true;
        return this;
    }
    public virtual ICreateVisitor WithBy(object insertObj)
    {
        var entityType = this.tables[0].EntityType;
        var commandInitializer = RepositoryHelper.BuildCreateWithBiesCommandInitializer(this, entityType, insertObj);
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        commandInitializer.Invoke(this, this.dbParameters, insertObj, fieldsBuilder, valuesBuilder);
        this.insertFields.Add(new InsertField
        {
            Type = InsertFieldType.Fields,
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
        return this;
    }
    public virtual ICreateVisitor WithBy(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        this.insertFields.Add(new InsertField
        {
            Type = InsertFieldType.Fields,
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = this.GetQuotedValue(fieldValue, memberMapper, true)
        });
        return this;
    }
    public virtual ICreateVisitor WithBulkFirst(object insertObjs)
    {
        var entityTppe = this.tables[0].EntityType;
        this.bulkCommandInitializer = RepositoryHelper.BuildCreateWithBulkCommandInitializer(this, entityTppe, insertObjs, out this.bulkHeadSql);
        return this;
    }
    public virtual ICreateVisitor WithBulk(IDbCommand command, StringBuilder builder, int index, object insertObj)
    {
        if (index == 0)
        {
            var tableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
            builder.Append($"{this.BuildHeadSql()} {tableName} ({this.bulkHeadSql}) VALUES ");
        }
        else builder.Append(',');
        builder.Append('(');
        this.bulkCommandInitializer.Invoke(command, this, builder, index, insertObj);
        builder.Append(')');
        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            builder.Append(tailSql);
        return this;
    }
    public virtual ICreateVisitor From(Expression fieldSelector)
    {
        if (isFrom) throw new NotSupportedException("INSERT INTO数据，只允许有一次From操作");
        var lambdaExpr = fieldSelector as LambdaExpression;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            var tableSegment = new TableSegment
            {
                EntityType = parameterExpr.Type,
                Mapper = this.MapProvider.GetEntityMap(parameterExpr.Type),
                AliasName = $"{(char)(this.TableAsStart + i)}"
            };
            this.tables.Add(tableSegment);
        }
        this.InitTableAlias(lambdaExpr);
        var sqlSegment = lambdaExpr.Body.NodeType switch
        {
            ExpressionType.New => this.VisitNew(new SqlSegment { Expression = lambdaExpr.Body }),
            ExpressionType.MemberInit => this.VisitMemberInit(new SqlSegment { Expression = lambdaExpr.Body }),
            _ => throw new NotImplementedException("不支持的表达式，只支持New或MemberInit表达式，如: new { a.Id, b.Name + &quot;xxx&quot; } 或是new User { Id = a.Id, Name = b.Name + &quot;xxx&quot; }")
        };
        var insertFields = (InsertField)sqlSegment.Value;
        var fromTablesBuilder = new StringBuilder();
        for (var i = 1; i < this.tables.Count; i++)
        {
            var tableSegment = this.tables[i];
            var tableName = tableSegment.Body;
            if (string.IsNullOrEmpty(tableName))
            {
                tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
            }
            if (i > 1) fromTablesBuilder.Append(',');
            fromTablesBuilder.Append(tableName + " " + tableSegment.AliasName);
        }
        insertFields.FromTables = fromTablesBuilder.ToString();
        this.insertFields.Add(insertFields);
        this.isFrom = true;
        return this;
    }
    public virtual ICreateVisitor Where(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.lastWhereNodeType = OperationType.None;
        this.whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public virtual ICreateVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
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
                //Where(f=>... && f.OrderNo.Length==10 && ...)
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
                //都需要带有别名
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

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        //这里不做参数化，后面统一走参数化处理 
        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.IsExpression = false;
        sqlSegment.IsMethodCall = false;
        return sqlSegment;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var fieldsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            var entityMapper = this.tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;

                this.AddMemberElement(i, new SqlSegment { Expression = newExpr.Arguments[i] }, memberMapper, fieldsBuilder, valuesBuilder);
            }
            return sqlSegment.ChangeValue(new InsertField
            {
                Type = InsertFieldType.FromTables,
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
        var entityMapper = this.tables[0].Mapper;
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
            Type = InsertFieldType.FromTables,
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.tableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            this.tableAlias.Add(parameterExpr.Name, this.tables[i + 1]);
        }
    }
    private void AddMemberElement(int index, SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder fieldsBuilder, StringBuilder valuesBuilder)
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
            valuesBuilder.Append(this.GetQuotedValue(sqlSegment));
        }
    }
}
public enum InsertFieldType
{
    Fields,
    FromTables,
}
public struct InsertField
{
    public InsertFieldType Type { get; set; }
    public string Fields { get; set; }
    public string Values { get; set; }
    public string FromTables { get; set; }
}