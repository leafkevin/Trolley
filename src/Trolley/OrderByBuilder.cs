using System;
using System.Linq.Expressions;

namespace Trolley;

public class OrderByBuilder
{
    private Expression expression = null;
    private string switchField;
    public bool IsAscending { get; set; }
    public void SwitchInternal(string switchValue, bool isAscending)
    {
        this.switchField = switchValue;
        this.IsAscending = isAscending;
    }
    public void WhenInternal(string whenValue, Expression fieldsSelector)
    {
        if (this.switchField != whenValue) return;
        this.expression = fieldsSelector;
    }
    public Expression Build() => this.expression;
}
public class OrderByBuilder<T> : OrderByBuilder
{
    public OrderByBuilder<T> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T> When<TFields>(string value, Expression<Func<T, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2> : OrderByBuilder
{
    public OrderByBuilder<T1, T2> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2> When<TFields>(string value, Expression<Func<T1, T2, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3> When<TFields>(string value, Expression<Func<T1, T2, T3, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}
public class OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : OrderByBuilder
{
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Switch(string switchValue, bool isAscending)
    {
        base.SwitchInternal(switchValue, isAscending);
        return this;
    }
    public OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> When<TFields>(string value, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsSelector)
    {
        base.WhenInternal(value, fieldsSelector);
        return this;
    }
}