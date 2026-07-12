using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public enum DeferredOperation : byte
{
    None = 0,
    Not,
    IsNull,
    IsTrue
}
public enum SqlType : byte
{
    FixedValue = 0,
    Constant,
    Variable,
    OnlyField,
    Expression,
    MethodCall,
    ReaderField,
    ReaderFields
}
[DebuggerDisplay("SqlType: {SqlType,nq} Value: {Value,nq} Expression: {Expression,nq}")]
public struct SqlSegment
{
    public static readonly SqlSegment True = new SqlSegment { IsTrue = true, SqlType = SqlType.FixedValue, Value = true };
    public static readonly SqlSegment Null = new SqlSegment { IsNull = true, SqlType = SqlType.FixedValue, Value = "NULL" };

    public Expression Expression { get; set; }
    public SqlType SqlType { get; set; }
    public object Value { get; set; }
    public bool IsDeferredFields { get; set; }
    /// <summary>
    /// 是否参数化当前值，本次解析有效
    /// </summary>
    public bool IsParameterized { get; set; }
    public bool IsEnum { get; set; }

    public bool IsTrue { get; set; }
    public bool IsNull { get; set; }
    public string ParameterName { get; set; }
    public TableSegment TableSegment { get; set; }
    public Type MappedTargetType { get; set; }
    public string MemberName { get; set; }
    //where条件时候，需要用于添加参数
    public MemberMap MemberMapper { get; set; }
    public ITypeHandler TypeHandler { get; set; }
    public MemberInfo TargetMember { get; set; }
    public List<ReaderField> Fields { get; set; }
    public bool IsRawSqlFields { get; set; }
    public bool IsValue => this.SqlType == SqlType.Constant || this.SqlType == SqlType.Variable;
    public bool HasField => this.SqlType > SqlType.Variable;
    public bool IsFixedValue => this.SqlType == SqlType.FixedValue;
    public bool HasDeferred => this.DeferredOperations != null && this.DeferredOperations.Count > 0;
    public Stack<DeferredOperation> DeferredOperations { get; set; }

    public SqlSegment Next(Expression expr)
    {
        this.Expression = expr;
        return this;
    }
    public SqlSegment Change(object value)
    {
        this.Value = value;
        return this;
    }
    public SqlSegment Change(object value, SqlType sqlType)
    {
        this.Value = value;
        this.SqlType = sqlType;
        return this;
    }
    public void Push(DeferredOperation deferredOperation)
    {
        this.DeferredOperations ??= new();
        this.DeferredOperations.Push(deferredOperation);
    }
    public bool HasNotOperation(out DeferredOperation lastOperation)
    {
        lastOperation = DeferredOperation.None;
        int notIndex = 0;
        while (this.DeferredOperations.Count > 0)
        {
            var operationType = this.DeferredOperations.Pop();
            switch (operationType)
            {
                case DeferredOperation.IsNull:
                case DeferredOperation.IsTrue:
                    lastOperation = operationType;
                    break;
                case DeferredOperation.Not:
                    notIndex++;
                    break;
            }
        }
        return notIndex % 2 > 0;
    }
    public string GetQuotedValue(IOrmProvider ormProvider)
    {
        if (this.IsNull || this.Value == null || this.Value is DBNull)
            return string.Empty;
        if (this.IsTrue) return ormProvider.GetQuotedValue(this.Value);
        return this.Value.ToString();
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplayText => $"SqlType: {this.SqlType} Value: {this.Value.ToString()} Expression: {this.Expression}";
}
public enum ReaderFieldType : byte
{
    /// <summary>
    /// 字段，原始字段
    /// </summary>
    Field,
    /// <summary>
    /// 实体类型，三种场景：参数访问，直接主表的Include导航属性成员访问，Grouping分组对象成员，返回的类型是ReaderField列表
    /// </summary>
    Entity,
    /// <summary>
    /// 1:1导航Include子表引用，场景: .Select(x => new { Order = x, CompanyInfo = x.Buyer.Company })
    /// </summary>
    IncludeRef,
    /// <summary>
    /// 1:N导航Include子表引用，场景: .Select(x => new { Orders = x.Orders})
    /// </summary>
    DeferredIncludeRef,
    /// <summary>
    /// 原始SQL
    /// </summary>
    RawSql,
    /// <summary>
    /// 常量、变量、参数、表达式计算、方法调用，非原始字段，需要AS别名
    /// </summary>
    Expression
}
public class ReaderField
{
    public ReaderFieldType FieldType { get; set; }
    public TableSegment TableSegment { get; set; }
    /// <summary>
    /// 包装后的SQL片段，字段名称、方法调用或是表达式SQL片段，只有是字段值的时候，值不做处理，只有到最后一步才处理
    /// </summary>
    public object Value { get; set; }
    /// <summary>
    /// FieldType类型为RawSql才有效，默认值是1
    /// </summary>
    public int FieldsCount { get; set; }
    public bool IsTargetType { get; set; }
    public Expression Expression { get; set; }
    /// <summary>
    /// 只需要在最外层select时设置
    /// </summary>
    public Type ReaderType { get; set; }
    public Type MappedTargetType { get; set; }
    /// <summary>
    /// 成员访问时是成员名称，临时表、多分表多次包装时也是成员名称
    /// </summary>
    public string MemberName { get; set; }
    //where条件时候，需要用于添加参数
    public object NativeDbType { get; set; }
    public ITypeHandler TypeHandler { get; set; }
    public MemberInfo TargetMember { get; set; }
    public bool IsDeferredFields { get; set; }
    /// <summary>
    /// 延迟字段中来自数据库字段的参数列表
    /// </summary>
    public List<ParameterExpression> FieldParameters { get; set; }
    /// <summary>
    /// 延迟字段中来自常量/闭包变量的参数列表
    /// </summary>
    public List<ParameterExpression> ValuesParameters { get; set; }
    public List<object> LocalValues { get; set; }
    public bool IsGroupingField { get; set; }
    public bool IsOrderingField { get; set; }
    public List<ReaderField> Fields { get; set; }
    public string Path { get; set; }
    public bool HasNextInclude { get; set; }
    public ReaderField Parent { get; set; }
    public ReaderField RefField { get; set; }
    public bool IsAggField { get; set; }
    public bool IsAvgField { get; set; }
    public string AggFunc { get; set; }
    public bool IsNeedAlias { get; set; }
    public string AliasName { get; set; }
    public bool IsIgnore { get; set; }

    public ReaderField Clone()
    {
        var result = (ReaderField)this.MemberwiseClone();
        if (this.Fields != null && this.Fields.Count > 0)
        {
            result.Fields = new();
            this.Fields.ForEach(f => result.Fields.Add(f.Clone()));
        }
        return result;
    }
}