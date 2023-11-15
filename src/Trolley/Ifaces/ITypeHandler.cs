﻿using System;
using System.Data;

namespace Trolley;

/// <summary>
/// 类型处理器，一般用在实体类中的非基础类型的成员，描述这个成员如何赋值和解析，如：
/// <example>
/// 类<c>Order</c>的属性ProductKinds，是一个数组,非基础类型，需要设置类型处理器，Trolley才知道如何处理
/// <code>
/// public class Order
/// {
///     public int Id { get; set; }
///     public string OrderNo { get; set; }
///     ...
///     public List&lt;int&gt; ProductKinds { get; set; }
/// }
/// </code>
/// public void OnModelCreating(ModelBuilder builder)
/// {
///     builder.Entity&lt;Order&gt;(f =&gt;
///     {
///          f.ToTable("sys_order").Key(t =&gt; t.Id);
///          f.Member(t =&gt; t.Id).Field(nameof(Order.Id)).NativeDbType(3);
///          f.Member(t =&gt; t.OrderNo).Field(nameof(Order.OrderNo)).NativeDbType(253);
///          ...
///          //数据库字段是JSON类型，保存时会转换成JSON保存到数据库中，取出来时，会把JSON转换为数组。
///          f.Member(t =&gt; t.ProductKinds).Field(nameof(Order.ProductKinds)).NativeDbType(245).SetTypeHandler&lt;JsonTypeHandler&gt;();
///     });
/// }
/// 栏位ProductKinds，数据库中字段是JSON类型，保存时会将其转换成JSON进行保存，从数据库取出来时，会把JSON转换为数组设置到实体中。
/// </example>
/// </summary>
public interface ITypeHandler
{
    object Parse(IOrmProvider ormProvider, Type TargetType, object value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ormProvider"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    object ToFieldValue(IOrmProvider ormProvider, object value);
}