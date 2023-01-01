namespace Trolley;

public interface IFromQuery
{
    IQuery<T> From<T>(char tableStartAs = 'a');
    IQuery<T1, T2> From<T1, T2>(char tableStartAs = 'a');
    IQuery<T1, T2, T3> From<T1, T2, T3>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableStartAs = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableStartAs = 'a');
}
