namespace Trolley;

public interface IAggregateSelect
{
    int Count();
    long LongCount();
    int Count<TField>(TField field);
    int CountDistinct<TField>(TField field);
    long LongCount<TField>(TField field);
    long LongCountDistinct<TField>(TField field);

    TField Sum<TField>(TField field);
    TTarget SumAs<TField, TTarget>(TField field);
    TField Avg<TField>(TField field);
    TTarget AvgAs<TField, TTarget>(TField field);
    TField Max<TField>(TField field);
    TTarget MaxAs<TField, TTarget>(TField field);
    TField Min<TField>(TField field);
    TTarget MinAs<TField, TTarget>(TField field);
}