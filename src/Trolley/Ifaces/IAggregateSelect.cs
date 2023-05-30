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

    /// <summary>
    /// 数据分组后，把字段field的多行数据，用separator字符分割拼接在一起，
    /// 行转列操作，只允许在最外层的SELECT语句中使用
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="field">字段名称</param>
    /// <param name="separator">连接符</param>
    /// <returns>返回连接后的字符串表达式</returns>
    //string GroupConcat<TField>(TField field, string separator = ",");
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <param name="table"></param>
    /// <returns></returns>
    //List<TTable> GroupInto<TTable>(TTable table);
}
