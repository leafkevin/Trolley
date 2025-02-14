using System;

namespace Trolley;

public static partial class Sql
{
    public static string ArrayToString<TField>(TField[] fields, string separator) => throw new NotImplementedException();

    public static IGroupConcat ArrayAgg<TFields>(TFields fields, string separator) => throw new NotImplementedException();

    public static IStringAgg StringAgg<TFields>(TFields fields, string separator) => throw new NotImplementedException();
}
public interface IStringAgg
{
    public interface ISqlOver<TValue>
    {
        ISqlOver<TValue> OrderBy<TFields>(TFields fields);
        ISqlOver<TValue> OrderByDescending<TFields>(TFields fields);
        IPartitionByOver<TValue> PartitionBy<TFields>(TFields fields);
        TValue ToValue();
    }
}