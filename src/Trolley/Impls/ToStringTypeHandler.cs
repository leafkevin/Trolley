using System;
using System.Data;

namespace Trolley;

public class ToStringTypeHandler : ITypeHandler
{
    public virtual void SetValue(IOrmProvider ormProvider, IDbDataParameter parameter, object value)
    {
        if (value == null)
            parameter.Value = DBNull.Value;
        else parameter.Value = value.ToString();
    }
    public virtual object Parse(IOrmProvider ormProvider, Type TargetType, object value)
    {
        if (value is DBNull) return null;
        return value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return value.ToString();
        return null;
    }
}
