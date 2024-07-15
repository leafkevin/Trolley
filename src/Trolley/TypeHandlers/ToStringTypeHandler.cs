﻿using System;

namespace Trolley;

public class ToStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DBNull)
            return null;
        return value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return value.ToString();
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
