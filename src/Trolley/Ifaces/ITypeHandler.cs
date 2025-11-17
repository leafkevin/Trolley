using System;

namespace Trolley;

public interface ITypeHandler
{
    object Parse(Type targetType, object value);
    object ToFieldValue(object value);
    string GetQuotedValue(object value);
}