using System;

namespace Trolley;

public interface ITypeHandler
{
    object Parse(IOrmProvider ormProvider, Type targetType, object value);
    object ToFieldValue(IOrmProvider ormProvider, object value);
    string GetQuotedValue(IOrmProvider ormProvider, object value);
}