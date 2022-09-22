using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

internal sealed partial class TheaRow : IDynamicMetaObjectProvider
{
    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
    {
        return new TheaRowMetaObject(parameter, BindingRestrictions.Empty, this);
    }
}
internal sealed class TheaRowMetaObject : DynamicMetaObject
{
    private static readonly MethodInfo getValueMethod = typeof(IDictionary<string, object>).GetProperty("Item").GetGetMethod();
    private static readonly MethodInfo setValueMethod = typeof(TheaRow).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) });

    public TheaRowMetaObject(Expression expression, BindingRestrictions restrictions)
        : base(expression, restrictions) { }

    public TheaRowMetaObject(Expression expression, BindingRestrictions restrictions, object value)
        : base(expression, restrictions, value) { }

    private DynamicMetaObject CallMethod(MethodInfo method, Expression[] parameters)
    {
        var callMethod = new DynamicMetaObject(Expression.Call(Expression.Convert(Expression, LimitType),
            method, parameters), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        return callMethod;
    }

    public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        => CallMethod(getValueMethod, new Expression[] { Expression.Constant(binder.Name) });
    // Needed for Visual basic dynamic support
    public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
        => CallMethod(getValueMethod, new Expression[] { Expression.Constant(binder.Name) });
    public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        => CallMethod(setValueMethod, new Expression[] { Expression.Constant(binder.Name), value.Expression });
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (HasValue && Value is IDictionary<string, object> lookup) return lookup.Keys;
        return Array.Empty<string>();
    }
}
