using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IQueryAnonymousObject
{
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 子查询，所有的子查询都是从From开始的
/// </summary>
public interface IFromQuery
{
    /// <summary>
    /// 创建子查询
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">子查询中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns></returns>
    IFromQuery<T> From<T>(char tableAsStart = 'a');
    /// <summary>
    /// 创建子查询
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="tableAsStart"></param>
    /// <returns></returns>
    IFromQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(char tableAsStart = 'a');
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(char tableAsStart = 'a');
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T>
{
    /// <summary>
    /// 子查询中的Union操作，用法：
    /// <code>
    /// await repository.From&lt;Order&gt;()
    ///     .Where(x => x.Id == 1)
    ///     .Select(x => new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///      })
    ///     .Union(f => f.From&lt;Order&gt;()
    ///         .Where(x => x.Id > 1)
    ///         .Select(x => new
    ///         {
    ///             x.Id,
    ///             x.OrderNo,
    ///             x.SellerId,
    ///             x.BuyerId
    ///         }))
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`=1 UNION
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    /// <code>
    /// f.From&lt;Order&gt;()
    ///     .Where(x => x.Id > 1)
    ///     .Select(x => new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     }
    /// </code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery);
    /// <summary>
    /// 子查询中的Union All操作，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Select(x =&gt; new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     })
    ///     .UnionAll(f =&gt; f
    ///         .From&lt;Order&gt;()
    ///         .Where(x =&gt; x.Id &gt; 1)
    ///         .Select(x =&gt; new
    ///         {
    ///             x.Id,
    ///             x.OrderNo,
    ///             x.SellerId,
    ///             x.BuyerId
    ///         }))
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`=1 UNION ALL
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    ///  f.From&lt;Order&gt;()
    ///     .Where(x =&gt; x.Id &gt; 1)
    ///     .Select(x =&gt; new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     }
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery);
    IFromQuery<T> UnionRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery);
    IFromQuery<T> UnionAllRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery);

    IFromQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    IFromQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    IFromQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    IFromQuery<T, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);
    IFromQuery<T, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);
    IFromQuery<T, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);

    IFromQuery<T> Where(Expression<Func<T, bool>> predicate = null);
    IFromQuery<T> And(bool condition, Expression<Func<T, bool>> predicate = null);
    IFromGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    IFromQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IFromQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IFromQuery<T> Distinct();
    IFromQuery<T> Select();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2>
{
    IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    IFromQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IFromQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IFromQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IFromQuery<T1, T2, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);

    IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate = null);
    IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    IFromQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    IFromQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    IFromQuery<T1, T2> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3>
{
    IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IFromQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate = null);
    IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4>
{
    IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5>
{
    IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6>
{
    IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);

    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate = null);
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate = null);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Distinct();
    IQueryAnonymousObject Select(string fields = "*");
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    string ToSql(out List<IDbDataParameter> dbParameters);
}