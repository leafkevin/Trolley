using System;

namespace Trolley;

#if !NETCOREAPP2_1_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
public struct HashCode
{
    private int hashCode;
    public HashCode() => hashCode = 17;
    public void Add<T>(T value)
    {
        unchecked
        {
            hashCode = hashCode * 23 + (value?.GetHashCode() ?? 0);
        }
    }
    public int ToHashCode() => hashCode;

    public static int Combine<T1>(T1 value1)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2>(T1 value1, T2 value2)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value4?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value4?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value5?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value4?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value5?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value6?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value4?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value5?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value6?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value7?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
    public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
    {
        unchecked
        {
            int hashCode = 17;
            hashCode = hashCode * 23 + (value1?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value2?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value3?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value4?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value5?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value6?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value7?.GetHashCode() ?? 0);
            hashCode = hashCode * 23 + (value8?.GetHashCode() ?? 0);
            return hashCode;
        }
    }
}
#endif