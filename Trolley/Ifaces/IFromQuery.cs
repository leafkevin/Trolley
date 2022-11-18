namespace Trolley;

public interface IFromQuery
{
    IQuery<T> From<T>();
    IQuery<T1, T2> From<T1, T2>();
    IQuery<T1, T2, T3> From<T1, T2, T3>();
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>();
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>();
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>();
}
