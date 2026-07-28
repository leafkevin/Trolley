using System;

namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    /// <summary>
    /// 表达式中使用变量默认的参数名前缀，默认值是p，如：@p1,@p2等
    /// </summary>
    public string UserParameterPrefix { get; set; } = "p";
    /// <summary>
    /// 表达式解析中，常量是否参数化。如果设置为true，所有常量也将都会参数化，所有变量都会做参数化处理。
    /// </summary>
    public bool IsConstantParameterized { get; set; } = false;
    /// <summary>
    /// 是否自动映射Json类型处理器，如果为true，并且实体成员是引用类型或是拥有多个字段、属性的结构，框架将自动使用JsonTypeHandler类型处理器进行映射，如果为false，需要用户自己手动指定映射Json类型处理器，默认值为true
    /// </summary>
    public bool IsAutoMapJsonTypeHandler { get; set; } = true;
    /// <summary>
    /// 枚举类型常量或变量，在未指定dbType类型时映射到数据库的默认类型，默认值是int类型
    /// </summary>
    public Type DefaultEnumMapDbType { get; set; } = typeof(int);
    /// <summary>
    /// DateTime、DateTimeOffset类型的DateTimeKind，默认是DateTimeKind.Local，如果返回的日期类型不是默认是DefaultDateTimeKind，将转换为DefaultDateTimeKind类型，如果值为DateTimeKind.Unspecified，将不做处理
    /// </summary>
    public DateTimeKind DefaultDateTimeKind { get; set; } = DateTimeKind.Local;

    internal void CopyTo(OrmDbFactoryOptions target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        this.CommandTimeout = target.CommandTimeout;
        this.UserParameterPrefix = target.UserParameterPrefix;
        this.IsConstantParameterized = target.IsConstantParameterized;
        this.IsAutoMapJsonTypeHandler = target.IsAutoMapJsonTypeHandler;
        this.DefaultEnumMapDbType = target.DefaultEnumMapDbType;
        this.DefaultDateTimeKind = target.DefaultDateTimeKind;
    }
}