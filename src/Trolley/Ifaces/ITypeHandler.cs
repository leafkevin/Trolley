using System;
using System.Data;

namespace Trolley;

public interface ITypeHandler
{
    void SetValue(IOrmProvider ormProvider, IDbDataParameter parameter, object value);
    void SetValue(IOrmProvider ormProvider, object value, out string sqlValue);
    object Parse(IOrmProvider ormProvider, Type TargetType, object value);
}