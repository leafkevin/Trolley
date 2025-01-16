using System;

namespace Trolley;

public interface IPartitionBy
{
    IPartitionBy OrderBy<TFields>(TFields fields);
    IPartitionBy OrderByDescending<TFields>(TFields fields);
    IPartitionByOver Over();
}
public interface IPartitionByOver
{
    int Rank();
    long LongRank();
    int DenseRank();
    long LongDenseRank();
    int RowNumber();
    int LongRowNumber();
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
}
