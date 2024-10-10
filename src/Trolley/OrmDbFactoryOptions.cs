using System;

namespace Trolley;

public class OrmDbFactoryOptions
{
    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    /// <summary>
    /// 表达式中使用变量默认的参数名前缀，默认值是p
    /// </summary>
    public string ParameterPrefix { get; set; } = "p";
    /// <summary>
    /// 表达式解析中，常量是否参数化。如果设置为true，所有常量也将都会参数化，所有变量都会做参数化处理。
    /// </summary>
    public bool IsConstantParameterized { get; set; } = false;
    /// <summary>
    /// 枚举类型常量或变量，在未指定dbType类型时映射到数据库的默认类型，默认值是int类型
    /// </summary>
    public Type DefaultEnumMapDbType { get; set; } = typeof(int);
    /// <summary>
    /// DateTime、DateTimeOffset类型的DateTimeKind，默认是DateTimeKind.Local，如果返回的日期类型不是默认是DefaultDateTimeKind，将转换为DefaultDateTimeKind类型，如果值为DateTimeKind.Unspecified，将不做处理
    /// </summary>
    public DateTimeKind DefaultDateTimeKind { get; set; } = DateTimeKind.Local;
    /// <summary>
    /// 拦截器，默认为null
    /// </summary>
    public DbInterceptors DbInterceptors { get; set; } = new DbInterceptors();
    /// <summary>
    /// 字段映射处理器，默认为DefaultFieldMapHandler实例
    /// </summary>
    public IFieldMapHandler FieldMapHandler { get; set; } = new DefaultFieldMapHandler();
}
