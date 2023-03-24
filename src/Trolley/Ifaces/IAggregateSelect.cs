namespace Trolley;

/// <summary>
/// 聚合查询，如：Count,Sum,Avg,Max,Min等
/// </summary>
public interface IAggregateSelect
{
    int Count();
    long LongCount();
    int Count<TField>(TField field);
    int CountDistinct<TField>(TField field);
    long LongCount<TField>(TField field);
    long LongCountDistinct<TField>(TField field);
    TField Sum<TField>(TField field);
    TField Avg<TField>(TField field);
    TField Max<TField>(TField field);
    TField Min<TField>(TField field);
    string GroupConcat<TField>(TField field, string separator = ",");
}
