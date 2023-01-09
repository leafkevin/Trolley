using System;
using System.Data;

namespace Trolley;

public class JsonTypeHandler : ITypeHandler
{
    public void SetValue(IOrmProvider ormProvider, int nativeDbType, object value, out IDbDataParameter parameter)
    {
        throw new NotImplementedException();
    }
    public void SetValue(IOrmProvider ormProvider, int nativeDbType, object value, out string sqlValue)
    {
        throw new NotImplementedException();
    }
    public object Parse(IOrmProvider ormProvider, int nativeDbType, Type TargetType, object value)
    {
        throw new NotImplementedException();
    }
}
