using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface ICreate<T>
{
    ICreate<T> WithBy<TInsertObject>(TInsertObject insertObjs);
    /// <summary>
    /// 批量执行选项设置，一般不需要使用该方法<para></para>
    /// 各数据库 values, parameters 限制不一样，默认设置：<para></para>
    /// MySql 5000 3000<para></para>
    /// PostgreSQL 5000 3000<para></para>
    /// SqlServer 1000 2100<para></para>
    /// Oracle 500 999<para></para>
    /// Sqlite 5000 999<para></para>
    /// 若没有事务传入，内部(默认)会自动开启新事务，保证拆包执行的完整性。
    /// </summary>
    /// <param name="valuesLimit">指定根据 values 上限数量拆分执行</param>
    /// <param name="parametersLimit">指定根据 parameters 上限数量拆分执行</param>
    /// <param name="isAutoTransaction">是否自动开启事务</param>
    /// <returns></returns>
    ICreate<T> WithBatch(int valuesLimit, int parametersLimit, bool isAutoTransaction = true);
    int Execute();
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    string ToSql();
}

public interface IMultilCreate<T>
{
    IMultilCreate<T> WithBy<TInsertObject>(TInsertObject insertObjs);
    /// <summary>
    /// 批量执行选项设置，一般不需要使用该方法<para></para>
    /// 各数据库 values, parameters 限制不一样，默认设置：<para></para>
    /// MySql 5000 3000<para></para>
    /// PostgreSQL 5000 3000<para></para>
    /// SqlServer 1000 2100<para></para>
    /// Oracle 500 999<para></para>
    /// Sqlite 5000 999<para></para>
    /// 若没有事务传入，内部(默认)会自动开启新事务，保证拆包执行的完整性。
    /// </summary>
    /// <param name="valuesLimit">指定根据 values 上限数量拆分执行</param>
    /// <param name="parametersLimit">指定根据 parameters 上限数量拆分执行</param>
    /// <param name="isAutoTransaction">是否自动开启事务</param>
    /// <returns></returns>
    ICreate<T> WithBatch(int valuesLimit, int parametersLimit, bool isAutoTransaction = true);
    //IMultiQuery Execute();
    string ToSql();
}