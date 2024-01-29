using System;

namespace Trolley;

public interface ITypeHandler
{
    object Parse(IOrmProvider ormProvider, Type underlyingType, object value);
    object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value);
    string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value);
}