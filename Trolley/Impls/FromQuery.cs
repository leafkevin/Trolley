using System;
using System.Collections.Generic;
using System.Data;

namespace Trolley;

public class FromQuery : IFromQuery
{
    private string parameterPrefix { get; set; }
    public FromQuery(string parameterPrefix)
    {
        this.parameterPrefix = parameterPrefix;
    }

    public IQuery<T> From<T>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2> From<T1, T2>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        throw new NotImplementedException();
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        throw new NotImplementedException();
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        throw new NotImplementedException();
    }
}
