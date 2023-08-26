using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static ICreate<TEntity> IgnoreInto<TEntity>(this ICreate<TEntity> createObj)
    {

        return createObj;
    }
    public static ICreate<TEntity> OnDuplicateKeyUpdate<TEntity>(this ICreate<TEntity> createObj)
    {

        return createObj;
    }
}
